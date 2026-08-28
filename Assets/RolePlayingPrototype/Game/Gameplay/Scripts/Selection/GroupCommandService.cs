using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;
using UnityEngine.AI;

namespace SampleProject
{
    public interface IGroupCommandService
    {
        void Move(Vector3 destination);
        void Attack(EntityHandle target);
        void Gather(EntityHandle resource);
        void Patrol(IReadOnlyList<Vector3> points);
        void Stop();
    }

    public sealed class GroupCommandService : IGroupCommandService
    {
        private const float FormationSpacing = 1.5f;
        private const float NavMeshProbeDistance = 0.15f;
        private const float NavMeshHeightTolerance = 0.25f;
        private const float MinimumSlotDistanceFactor = 0.8f;
        private const float NavigationSampleDistance = 2f;

        private readonly IUnitSelectionService _selection;
        private readonly IEntityCommandService _commands;
        private readonly EcsWorld _world;
        private readonly List<EntityHandle> _remainingUnits = new();
        private readonly List<Vector3> _formationSlots = new();
        private readonly List<Vector3> _adaptiveSlots = new();
        private readonly List<GridSlot> _gridSlots = new();
        private readonly Dictionary<long, int> _gridSlotIndices = new();
        private readonly HashSet<long> _originalGridSlots = new();
        private readonly HashSet<long> _acceptedGridSlots = new();
        private readonly HashSet<long> _visitedGridSlots = new();
        private readonly Queue<long> _gridSlotQueue = new();
        private readonly List<long> _currentGridComponent = new();
        private readonly List<long> _largestGridComponent = new();
        private readonly INavigationPathService _navigation;
        private readonly IFormationPlannerService _formationPlanner;
        private readonly List<FormationUnit> _formationUnits = new();
        private float[,] _assignmentCosts;
        private float[] _unitPotentials;
        private float[] _slotPotentials;
        private float[] _minimumCosts;
        private int[] _slotMatching;
        private int[] _previousSlots;
        private bool[] _usedSlots;
        private int _assignmentCapacity;

        public GroupCommandService(IUnitSelectionService selection, IEntityCommandService commands, EcsWorld world, INavigationPathService navigation, IFormationPlannerService formationPlanner)
        {
            _selection = selection;
            _commands = commands;
            _world = world;
            _navigation = navigation;
            _formationPlanner = formationPlanner;
        }

        public void Move(Vector3 destination)
        {
            var transformPool = _world.GetPool<TransformComponent>();
            _remainingUnits.Clear();
            var groupCenter = Vector3.zero;
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                var unit = _selection.Selected[i];
                if (!_world.IsEntityExists(unit) || !transformPool.HasComponent(unit.Id))
                {
                    continue;
                }

                _remainingUnits.Add(unit);
                groupCenter += transformPool.GetComponent(unit.Id).Value.position;
            }

            var count = _remainingUnits.Count;
            if (count == 0)
            {
                return;
            }

            groupCenter /= count;
            var forward = Vector3.ProjectOnPlane(destination - groupCenter, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
            var rows = Mathf.Max(1, Mathf.CeilToInt((float)count / columns));
            var navigationWaypoints = BuildNavigationWaypoints(groupCenter, destination);

            _formationUnits.Clear();
            for (var i = 0; i < count; i++)
            {
                var unit = _remainingUnits[i];
                _formationUnits.Add(new FormationUnit(unit, transformPool.GetComponent(unit.Id).Value.position));
            }

            if (_formationPlanner.TryBuild(_formationUnits, destination, forward, out var destinations))
            {
                for (var i = 0; i < count; i++)
                {
                    var unit = _remainingUnits[i];
                    var unitPosition = transformPool.GetComponent(unit.Id).Value.position;
                    var unitDestination = destinations[unit];
                    var unitWaypoints = ResolveNavigationWaypoints(unitPosition, unitDestination, navigationWaypoints);
                    _commands.Move(unit, unitDestination, unitWaypoints);
                }

                return;
            }

            _formationSlots.Clear();
            for (var row = 0; row < rows; row++)
            {
                var unitsInRow = Mathf.Min(columns, count - row * columns);
                var forwardOffset = ((rows - 1) * 0.5f - row) * FormationSpacing;
                for (var column = 0; column < unitsInRow; column++)
                {
                    var rightOffset = (column - (unitsInRow - 1) * 0.5f) * FormationSpacing;
                    _formationSlots.Add(destination + forward * forwardOffset + right * rightOffset);
                }
            }

            _remainingUnits.Sort((left, rightUnit) => ProjectPosition(rightUnit, groupCenter, forward).CompareTo(ProjectPosition(left, groupCenter, forward)));
            var lateralComparer = Comparer<EntityHandle>.Create((left, rightUnit) => ProjectPosition(left, groupCenter, right).CompareTo(ProjectPosition(rightUnit, groupCenter, right)));
            for (var row = 0; row < rows; row++)
            {
                var rowStart = row * columns;
                var unitsInRow = Mathf.Min(columns, count - rowStart);
                _remainingUnits.Sort(rowStart, unitsInRow, lateralComparer);
            }

            if (TryBuildAdaptiveFormation(destination, forward, right, columns, rows, count))
            {
                AssignAdaptiveFormation(transformPool, navigationWaypoints);
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var unitPosition = transformPool.GetComponent(_remainingUnits[i].Id).Value.position;
                var unitWaypoints = ResolveNavigationWaypoints(unitPosition, _formationSlots[i], navigationWaypoints);
                _commands.Move(_remainingUnits[i], _formationSlots[i], unitWaypoints);
            }
        }

        private IReadOnlyList<Vector3> BuildNavigationWaypoints(Vector3 start, Vector3 destination)
        {
            if (!_navigation.TryBuildPath(start, destination, NavigationSampleDistance, out var path) || !path.IsComplete)
            {
                return null;
            }

            var corners = path.Corners;
            if (corners.Length <= 2)
            {
                return null;
            }

            var waypoints = new List<Vector3>(corners.Length - 2);
            for (var i = 1; i < corners.Length - 1; i++)
            {
                waypoints.Add(corners[i]);
            }

            return waypoints;
        }

        private IReadOnlyList<Vector3> ResolveNavigationWaypoints(Vector3 start, Vector3 destination, IReadOnlyList<Vector3> sharedWaypoints)
        {
            if (sharedWaypoints == null || sharedWaypoints.Count == 0)
            {
                return _navigation.HasDirectPath(start, destination, NavigationSampleDistance) ? null : BuildNavigationWaypoints(start, destination);
            }

            var firstSegmentBlocked = !_navigation.HasDirectPath(start, sharedWaypoints[0], NavigationSampleDistance);
            var lastSegmentBlocked = !_navigation.HasDirectPath(sharedWaypoints[sharedWaypoints.Count - 1], destination, NavigationSampleDistance);
            return firstSegmentBlocked || lastSegmentBlocked ? BuildNavigationWaypoints(start, destination) : sharedWaypoints;
        }

        private bool TryBuildAdaptiveFormation(Vector3 destination, Vector3 forward, Vector3 right, int columns, int rows, int count)
        {
            if (!NavMesh.SamplePosition(destination, out _, 1f, NavMesh.AllAreas))
            {
                return false;
            }

            _gridSlots.Clear();
            _gridSlotIndices.Clear();
            _originalGridSlots.Clear();
            var blockedSlotFound = false;
            var formationSlotIndex = 0;
            for (var row = 0; row < rows; row++)
            {
                var unitsInRow = Mathf.Min(columns, count - row * columns);
                var firstColumn = (columns - unitsInRow) / 2;
                for (var column = firstColumn; column < firstColumn + unitsInRow; column++)
                {
                    var candidate = _formationSlots[formationSlotIndex++];
                    if (TryResolveSlot(candidate, out var resolvedSlot))
                    {
                        AddGridSlot(column, row, resolvedSlot, true);
                    }
                    else
                    {
                        blockedSlotFound = true;
                    }
                }
            }

            if (!blockedSlotFound)
            {
                return false;
            }

            var maximumExpansion = Mathf.CeilToInt(Mathf.Sqrt(count)) + 4;
            for (var expansion = 1; expansion <= maximumExpansion; expansion++)
            {
                var minimumColumn = -expansion;
                var maximumColumn = columns - 1 + expansion;
                var minimumRow = -expansion;
                var maximumRow = rows - 1 + expansion;
                for (var row = minimumRow; row <= maximumRow; row++)
                {
                    for (var column = minimumColumn; column <= maximumColumn; column++)
                    {
                        if (row != minimumRow && row != maximumRow && column != minimumColumn && column != maximumColumn)
                        {
                            continue;
                        }

                        var candidate = GetGridPosition(destination, forward, right, columns, rows, column, row);
                        if (TryResolveSlot(candidate, out var resolvedSlot))
                        {
                            AddGridSlot(column, row, resolvedSlot, false);
                        }
                    }
                }

                if (_gridSlots.Count < count)
                {
                    continue;
                }

                BuildConnectedAdaptiveFormation(destination, columns, rows, count);
                if (_adaptiveSlots.Count == count)
                {
                    return true;
                }
            }

            BuildConnectedAdaptiveFormation(destination, columns, rows, count);
            if (_adaptiveSlots.Count == count)
            {
                return true;
            }

            AddDisconnectedSlots(destination, columns, rows, count);
            return _adaptiveSlots.Count == count;
        }

        private void BuildConnectedAdaptiveFormation(Vector3 destination, int columns, int rows, int count)
        {
            _adaptiveSlots.Clear();
            _acceptedGridSlots.Clear();
            FindLargestOriginalComponent();
            if (_largestGridComponent.Count == 0)
            {
                var closestSlot = 0;
                var closestDistance = float.MaxValue;
                for (var i = 0; i < _gridSlots.Count; i++)
                {
                    var distance = (_gridSlots[i].Position - destination).sqrMagnitude;
                    if (distance < closestDistance)
                    {
                        closestSlot = i;
                        closestDistance = distance;
                    }
                }

                if (_gridSlots.Count > 0)
                {
                    AcceptGridSlot(GetGridKey(_gridSlots[closestSlot].Column, _gridSlots[closestSlot].Row));
                }
            }

            for (var i = 0; i < _largestGridComponent.Count; i++)
            {
                AcceptGridSlot(_largestGridComponent[i]);
            }

            while (_adaptiveSlots.Count < count)
            {
                var bestKey = 0L;
                var bestNeighbours = -1;
                var bestExpansion = int.MaxValue;
                var bestDistance = float.MaxValue;
                for (var i = 0; i < _gridSlots.Count; i++)
                {
                    var slot = _gridSlots[i];
                    var key = GetGridKey(slot.Column, slot.Row);
                    if (_acceptedGridSlots.Contains(key))
                    {
                        continue;
                    }

                    var neighbours = CountAcceptedNeighbours(slot.Column, slot.Row);
                    if (neighbours == 0)
                    {
                        continue;
                    }

                    var expansion = GetExpansion(slot.Column, slot.Row, columns, rows);
                    var distance = (slot.Position - destination).sqrMagnitude;
                    if (neighbours > bestNeighbours || neighbours == bestNeighbours && (expansion < bestExpansion || expansion == bestExpansion && distance < bestDistance))
                    {
                        bestKey = key;
                        bestNeighbours = neighbours;
                        bestExpansion = expansion;
                        bestDistance = distance;
                    }
                }

                if (bestNeighbours < 0)
                {
                    break;
                }

                AcceptGridSlot(bestKey);
            }
        }

        private void FindLargestOriginalComponent()
        {
            _visitedGridSlots.Clear();
            _largestGridComponent.Clear();
            foreach (var startKey in _originalGridSlots)
            {
                if (!_visitedGridSlots.Add(startKey))
                {
                    continue;
                }

                _currentGridComponent.Clear();
                _gridSlotQueue.Clear();
                _gridSlotQueue.Enqueue(startKey);
                while (_gridSlotQueue.Count > 0)
                {
                    var key = _gridSlotQueue.Dequeue();
                    _currentGridComponent.Add(key);
                    var slot = _gridSlots[_gridSlotIndices[key]];
                    for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
                    {
                        for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
                        {
                            if (columnOffset == 0 && rowOffset == 0)
                            {
                                continue;
                            }

                            var neighbourKey = GetGridKey(slot.Column + columnOffset, slot.Row + rowOffset);
                            if (_originalGridSlots.Contains(neighbourKey) && _visitedGridSlots.Add(neighbourKey))
                            {
                                _gridSlotQueue.Enqueue(neighbourKey);
                            }
                        }
                    }
                }

                if (_currentGridComponent.Count > _largestGridComponent.Count)
                {
                    _largestGridComponent.Clear();
                    _largestGridComponent.AddRange(_currentGridComponent);
                }
            }
        }

        private void AddDisconnectedSlots(Vector3 destination, int columns, int rows, int count)
        {
            while (_adaptiveSlots.Count < count)
            {
                var bestKey = 0L;
                var bestExpansion = int.MaxValue;
                var bestDistance = float.MaxValue;
                for (var i = 0; i < _gridSlots.Count; i++)
                {
                    var slot = _gridSlots[i];
                    var key = GetGridKey(slot.Column, slot.Row);
                    if (_acceptedGridSlots.Contains(key))
                    {
                        continue;
                    }

                    var expansion = GetExpansion(slot.Column, slot.Row, columns, rows);
                    var distance = (slot.Position - destination).sqrMagnitude;
                    if (expansion < bestExpansion || expansion == bestExpansion && distance < bestDistance)
                    {
                        bestKey = key;
                        bestExpansion = expansion;
                        bestDistance = distance;
                    }
                }

                if (bestExpansion == int.MaxValue)
                {
                    break;
                }

                AcceptGridSlot(bestKey);
            }
        }

        private void AddGridSlot(int column, int row, Vector3 position, bool isOriginal)
        {
            var key = GetGridKey(column, row);
            _gridSlotIndices.Add(key, _gridSlots.Count);
            _gridSlots.Add(new GridSlot(column, row, position));
            if (isOriginal)
            {
                _originalGridSlots.Add(key);
            }
        }

        private void AcceptGridSlot(long key)
        {
            if (!_acceptedGridSlots.Add(key))
            {
                return;
            }

            AddSlotIfSeparated(_gridSlots[_gridSlotIndices[key]].Position);
        }

        private int CountAcceptedNeighbours(int column, int row)
        {
            var count = 0;
            for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    if ((columnOffset != 0 || rowOffset != 0) && _acceptedGridSlots.Contains(GetGridKey(column + columnOffset, row + rowOffset)))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private int GetExpansion(int column, int row, int columns, int rows)
        {
            var columnExpansion = Mathf.Max(0, -column, column - columns + 1);
            var rowExpansion = Mathf.Max(0, -row, row - rows + 1);
            return Mathf.Max(columnExpansion, rowExpansion);
        }

        private Vector3 GetGridPosition(Vector3 destination, Vector3 forward, Vector3 right, int columns, int rows, int column, int row)
        {
            var forwardOffset = ((rows - 1) * 0.5f - row) * FormationSpacing;
            var rightOffset = (column - (columns - 1) * 0.5f) * FormationSpacing;
            return destination + forward * forwardOffset + right * rightOffset;
        }

        private long GetGridKey(int column, int row)
        {
            return (long)column << 32 | (uint)row;
        }

        private bool TryResolveSlot(Vector3 slot, out Vector3 resolvedSlot)
        {
            resolvedSlot = default;
            if (!NavMesh.SamplePosition(slot, out var hit, NavMeshProbeDistance, NavMesh.AllAreas))
            {
                return false;
            }

            var horizontalOffset = Vector3.ProjectOnPlane(hit.position - slot, Vector3.up);
            if (horizontalOffset.sqrMagnitude > NavMeshProbeDistance * NavMeshProbeDistance || Mathf.Abs(hit.position.y - slot.y) > NavMeshHeightTolerance)
            {
                return false;
            }

            resolvedSlot = hit.position;
            return true;
        }

        private void AddSlotIfSeparated(Vector3 slot)
        {
            var minimumDistance = FormationSpacing * MinimumSlotDistanceFactor;
            for (var i = 0; i < _adaptiveSlots.Count; i++)
            {
                if ((_adaptiveSlots[i] - slot).sqrMagnitude < minimumDistance * minimumDistance)
                {
                    return;
                }
            }

            _adaptiveSlots.Add(slot);
        }

        private void AssignAdaptiveFormation(EcsPool<TransformComponent> transformPool, IReadOnlyList<Vector3> navigationWaypoints)
        {
            var count = _remainingUnits.Count;
            EnsureAssignmentCapacity(count);
            for (var unitIndex = 0; unitIndex < count; unitIndex++)
            {
                var unitPosition = transformPool.GetComponent(_remainingUnits[unitIndex].Id).Value.position;
                for (var slotIndex = 0; slotIndex < count; slotIndex++)
                {
                    _assignmentCosts[unitIndex + 1, slotIndex + 1] = (unitPosition - _adaptiveSlots[slotIndex]).sqrMagnitude;
                }
            }

            for (var index = 0; index <= count; index++)
            {
                _unitPotentials[index] = 0f;
                _slotPotentials[index] = 0f;
                _slotMatching[index] = 0;
            }

            for (var unit = 1; unit <= count; unit++)
            {
                _slotMatching[0] = unit;
                var currentSlot = 0;
                for (var slot = 0; slot <= count; slot++)
                {
                    _minimumCosts[slot] = float.MaxValue;
                    _usedSlots[slot] = false;
                }

                do
                {
                    _usedSlots[currentSlot] = true;
                    var currentUnit = _slotMatching[currentSlot];
                    var delta = float.MaxValue;
                    var nextSlot = 0;
                    for (var slot = 1; slot <= count; slot++)
                    {
                        if (_usedSlots[slot])
                        {
                            continue;
                        }

                        var cost = _assignmentCosts[currentUnit, slot] - _unitPotentials[currentUnit] - _slotPotentials[slot];
                        if (cost < _minimumCosts[slot])
                        {
                            _minimumCosts[slot] = cost;
                            _previousSlots[slot] = currentSlot;
                        }

                        if (_minimumCosts[slot] < delta)
                        {
                            delta = _minimumCosts[slot];
                            nextSlot = slot;
                        }
                    }

                    for (var slot = 0; slot <= count; slot++)
                    {
                        if (_usedSlots[slot])
                        {
                            _unitPotentials[_slotMatching[slot]] += delta;
                            _slotPotentials[slot] -= delta;
                        }
                        else
                        {
                            _minimumCosts[slot] -= delta;
                        }
                    }

                    currentSlot = nextSlot;
                }
                while (_slotMatching[currentSlot] != 0);

                do
                {
                    var previousSlot = _previousSlots[currentSlot];
                    _slotMatching[currentSlot] = _slotMatching[previousSlot];
                    currentSlot = previousSlot;
                }
                while (currentSlot != 0);
            }

            for (var slot = 1; slot <= count; slot++)
            {
                var unit = _remainingUnits[_slotMatching[slot] - 1];
                var destination = _adaptiveSlots[slot - 1];
                var unitPosition = transformPool.GetComponent(unit.Id).Value.position;
                var unitWaypoints = ResolveNavigationWaypoints(unitPosition, destination, navigationWaypoints);
                _commands.Move(unit, destination, unitWaypoints);
            }
        }

        private void EnsureAssignmentCapacity(int count)
        {
            if (_assignmentCapacity >= count)
            {
                return;
            }

            _assignmentCapacity = count;
            _assignmentCosts = new float[count + 1, count + 1];
            _unitPotentials = new float[count + 1];
            _slotPotentials = new float[count + 1];
            _minimumCosts = new float[count + 1];
            _slotMatching = new int[count + 1];
            _previousSlots = new int[count + 1];
            _usedSlots = new bool[count + 1];
        }

        private float ProjectPosition(EntityHandle unit, Vector3 origin, Vector3 axis)
        {
            var position = _world.GetPool<TransformComponent>().GetComponent(unit.Id).Value.position;
            return Vector3.Dot(position - origin, axis);
        }

        private readonly struct GridSlot
        {
            public readonly int Column;
            public readonly int Row;
            public readonly Vector3 Position;

            public GridSlot(int column, int row, Vector3 position)
            {
                Column = column;
                Row = row;
                Position = position;
            }
        }

        public void Attack(EntityHandle target)
        {
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                _commands.Attack(_selection.Selected[i], target);
            }
        }

        public void Gather(EntityHandle resource)
        {
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                _commands.Gather(_selection.Selected[i], resource);
            }
        }

        public void Patrol(IReadOnlyList<Vector3> points)
        {
            var group = new PatrolGroupState(new List<Vector3>(points));
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                _commands.Patrol(_selection.Selected[i], group.Points, group);
            }
        }

        public void Stop()
        {
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                _commands.Stop(_selection.Selected[i]);
            }
        }
    }
}
