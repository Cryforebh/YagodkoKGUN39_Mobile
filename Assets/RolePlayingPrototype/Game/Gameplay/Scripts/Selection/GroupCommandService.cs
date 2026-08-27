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

        private readonly IUnitSelectionService _selection;
        private readonly IEntityCommandService _commands;
        private readonly EcsWorld _world;
        private readonly List<EntityHandle> _remainingUnits = new();
        private readonly List<Vector3> _formationSlots = new();
        private readonly List<Vector3> _adaptiveSlots = new();
        private float[,] _assignmentCosts;
        private float[] _unitPotentials;
        private float[] _slotPotentials;
        private float[] _minimumCosts;
        private int[] _slotMatching;
        private int[] _previousSlots;
        private bool[] _usedSlots;
        private int _assignmentCapacity;

        public GroupCommandService(IUnitSelectionService selection, IEntityCommandService commands, EcsWorld world)
        {
            _selection = selection;
            _commands = commands;
            _world = world;
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
                AssignAdaptiveFormation(transformPool);
                return;
            }

            for (var i = 0; i < count; i++)
            {
                _commands.Move(_remainingUnits[i], _formationSlots[i]);
            }
        }

        private bool TryBuildAdaptiveFormation(Vector3 destination, Vector3 forward, Vector3 right, int columns, int rows, int count)
        {
            if (!NavMesh.SamplePosition(destination, out _, 1f, NavMesh.AllAreas))
            {
                return false;
            }

            _adaptiveSlots.Clear();
            var blockedSlotFound = false;
            for (var i = 0; i < _formationSlots.Count; i++)
            {
                if (TryResolveSlot(_formationSlots[i], out var resolvedSlot))
                {
                    AddSlotIfSeparated(resolvedSlot);
                }
                else
                {
                    blockedSlotFound = true;
                }
            }

            if (!blockedSlotFound)
            {
                return false;
            }

            var maximumExpansion = Mathf.CeilToInt(Mathf.Sqrt(count)) + 4;
            for (var expansion = 1; expansion <= maximumExpansion && _adaptiveSlots.Count < count; expansion++)
            {
                var minimumColumn = -expansion;
                var maximumColumn = columns - 1 + expansion;
                var minimumRow = -expansion;
                var maximumRow = rows - 1 + expansion;
                for (var row = minimumRow; row <= maximumRow && _adaptiveSlots.Count < count; row++)
                {
                    for (var column = minimumColumn; column <= maximumColumn && _adaptiveSlots.Count < count; column++)
                    {
                        if (row != minimumRow && row != maximumRow && column != minimumColumn && column != maximumColumn)
                        {
                            continue;
                        }

                        var forwardOffset = ((rows - 1) * 0.5f - row) * FormationSpacing;
                        var rightOffset = (column - (columns - 1) * 0.5f) * FormationSpacing;
                        var candidate = destination + forward * forwardOffset + right * rightOffset;
                        if (TryResolveSlot(candidate, out var resolvedSlot))
                        {
                            AddSlotIfSeparated(resolvedSlot);
                        }
                    }
                }
            }

            return _adaptiveSlots.Count == count;
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

        private void AssignAdaptiveFormation(EcsPool<TransformComponent> transformPool)
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
                _commands.Move(_remainingUnits[_slotMatching[slot] - 1], _adaptiveSlots[slot - 1]);
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
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                _commands.Patrol(_selection.Selected[i], points);
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
