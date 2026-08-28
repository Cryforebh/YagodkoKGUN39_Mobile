using System.Collections.Generic;
using GameECS;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GameEngine.Ecs
{
    public readonly struct FormationUnit
    {
        public EntityHandle Entity { get; }
        public Vector3 Position { get; }

        public FormationUnit(EntityHandle entity, Vector3 position)
        {
            Entity = entity;
            Position = position;
        }
    }

    public interface IFormationPlannerService
    {
        bool TryBuild(IReadOnlyList<FormationUnit> units, Vector3 center, Vector3 forward, out IReadOnlyDictionary<EntityHandle, Vector3> destinations);
    }

    public sealed class FormationPlannerService : IFormationPlannerService
    {
        private const float Spacing = 1.5f;
        private const float SampleDistance = 0.2f;
        private const float HeightTolerance = 0.25f;

        private readonly List<GridSlot> _gridSlots = new();
        private readonly Dictionary<long, int> _gridSlotIndices = new();
        private readonly HashSet<long> _accepted = new();
        private readonly List<Vector3> _slots = new();
        private readonly List<FormationUnit> _orderedUnits = new();
        private readonly Dictionary<EntityHandle, Vector3> _destinations = new();

        public bool TryBuild(IReadOnlyList<FormationUnit> units, Vector3 center, Vector3 forward, out IReadOnlyDictionary<EntityHandle, Vector3> destinations)
        {
            _gridSlots.Clear();
            _gridSlotIndices.Clear();
            _slots.Clear();
            _orderedUnits.Clear();
            _destinations.Clear();
            destinations = _destinations;
            if (units == null || units.Count == 0)
            {
                return false;
            }

            forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(units.Count)));
            var rows = Mathf.Max(1, Mathf.CeilToInt((float) units.Count / columns));
            var maximumExpansion = Mathf.CeilToInt(Mathf.Sqrt(units.Count)) + 12;
            for (var expansion = 0; expansion <= maximumExpansion; expansion++)
            {
                AddExpansion(center, forward, right, columns, rows, expansion);
                BuildConnectedSlots(center, -forward, columns, rows, units.Count);
                if (_slots.Count == units.Count)
                {
                    break;
                }
            }

            if (_slots.Count < units.Count)
            {
                return false;
            }

            for (var i = 0; i < units.Count; i++)
            {
                _orderedUnits.Add(units[i]);
            }

            _orderedUnits.Sort((left, rightUnit) => Vector3.Dot(rightUnit.Position - center, forward).CompareTo(Vector3.Dot(left.Position - center, forward)));
            _slots.Sort((left, rightSlot) => Vector3.Dot(rightSlot - center, forward).CompareTo(Vector3.Dot(left - center, forward)));
            var unitLateralComparer = Comparer<FormationUnit>.Create((left, rightUnit) => Vector3.Dot(left.Position - center, right).CompareTo(Vector3.Dot(rightUnit.Position - center, right)));
            var slotLateralComparer = Comparer<Vector3>.Create((left, rightSlot) => Vector3.Dot(left - center, right).CompareTo(Vector3.Dot(rightSlot - center, right)));
            for (var row = 0; row * columns < units.Count; row++)
            {
                var start = row * columns;
                var rowCount = Mathf.Min(columns, units.Count - start);
                _orderedUnits.Sort(start, rowCount, unitLateralComparer);
                _slots.Sort(start, rowCount, slotLateralComparer);
            }

            for (var i = 0; i < units.Count; i++)
            {
                _destinations.Add(_orderedUnits[i].Entity, _slots[i]);
            }

            return true;
        }

        private void AddExpansion(Vector3 center, Vector3 forward, Vector3 right, int columns, int rows, int expansion)
        {
            for (var row = -expansion; row < rows + expansion; row++)
            {
                for (var column = -expansion; column < columns + expansion; column++)
                {
                    if (expansion > 0 && row != -expansion && row != rows + expansion - 1 && column != -expansion && column != columns + expansion - 1)
                    {
                        continue;
                    }

                    var key = GetGridKey(column, row);
                    if (_gridSlotIndices.ContainsKey(key))
                    {
                        continue;
                    }

                    var forwardOffset = ((rows - 1) * 0.5f - row) * Spacing;
                    var rightOffset = (column - (columns - 1) * 0.5f) * Spacing;
                    var candidate = center + forward * forwardOffset + right * rightOffset;
                    if (!NavMesh.SamplePosition(candidate, out var hit, SampleDistance, NavMesh.AllAreas))
                    {
                        continue;
                    }

                    var horizontalOffset = Vector3.ProjectOnPlane(hit.position - candidate, Vector3.up);
                    if (horizontalOffset.sqrMagnitude > SampleDistance * SampleDistance || Mathf.Abs(hit.position.y - candidate.y) > HeightTolerance)
                    {
                        continue;
                    }

                    _gridSlotIndices.Add(key, _gridSlots.Count);
                    _gridSlots.Add(new GridSlot(column, row, hit.position));
                }
            }
        }

        private void BuildConnectedSlots(Vector3 center, Vector3 approachDirection, int columns, int rows, int count)
        {
            _slots.Clear();
            _accepted.Clear();
            if (_gridSlots.Count == 0)
            {
                return;
            }

            var seedIndex = 0;
            var seedScore = float.MaxValue;
            for (var i = 0; i < _gridSlots.Count; i++)
            {
                var offset = Vector3.ProjectOnPlane(_gridSlots[i].Position - center, Vector3.up);
                var oppositeSidePenalty = Vector3.Dot(offset, approachDirection) < 0f ? 10000f : 0f;
                var score = offset.sqrMagnitude + oppositeSidePenalty;
                if (score < seedScore)
                {
                    seedIndex = i;
                    seedScore = score;
                }
            }

            var seed = _gridSlots[seedIndex];
            var seedKey = GetGridKey(seed.Column, seed.Row);
            _accepted.Add(seedKey);
            _slots.Add(seed.Position);
            var minimumColumn = seed.Column;
            var maximumColumn = seed.Column;
            var minimumRow = seed.Row;
            var maximumRow = seed.Row;
            var desiredCenterColumn = (columns - 1) * 0.5f;
            var desiredCenterRow = (rows - 1) * 0.5f;
            while (_slots.Count < count)
            {
                var bestIndex = -1;
                var bestScore = float.MaxValue;
                for (var i = 0; i < _gridSlots.Count; i++)
                {
                    var candidate = _gridSlots[i];
                    var candidateKey = GetGridKey(candidate.Column, candidate.Row);
                    if (_accepted.Contains(candidateKey) || NavMesh.Raycast(seed.Position, candidate.Position, out _, NavMesh.AllAreas))
                    {
                        continue;
                    }

                    var neighbourCount = CountConnectedNeighbours(candidate);
                    if (neighbourCount == 0)
                    {
                        continue;
                    }

                    var candidateMinimumColumn = Mathf.Min(minimumColumn, candidate.Column);
                    var candidateMaximumColumn = Mathf.Max(maximumColumn, candidate.Column);
                    var candidateMinimumRow = Mathf.Min(minimumRow, candidate.Row);
                    var candidateMaximumRow = Mathf.Max(maximumRow, candidate.Row);
                    var width = candidateMaximumColumn - candidateMinimumColumn + 1;
                    var height = candidateMaximumRow - candidateMinimumRow + 1;
                    var emptyCells = width * height - (_slots.Count + 1);
                    var oversizedColumns = Mathf.Max(0, width - columns);
                    var oversizedRows = Mathf.Max(0, height - rows);
                    var normalizedWidth = (float) width / columns;
                    var normalizedHeight = (float) height / rows;
                    var shapeDistortion = Mathf.Abs(normalizedWidth - normalizedHeight);
                    var boundsCenterColumn = (candidateMinimumColumn + candidateMaximumColumn) * 0.5f;
                    var boundsCenterRow = (candidateMinimumRow + candidateMaximumRow) * 0.5f;
                    var centerOffset = Mathf.Abs(boundsCenterColumn - desiredCenterColumn) + Mathf.Abs(boundsCenterRow - desiredCenterRow);
                    var worldCenterDistance = Vector3.ProjectOnPlane(candidate.Position - center, Vector3.up).sqrMagnitude;
                    var score = (oversizedColumns + oversizedRows) * 10000f + shapeDistortion * 1000f + emptyCells * 100f + centerOffset * 10f + worldCenterDistance - neighbourCount;
                    if (score >= bestScore)
                    {
                        continue;
                    }

                    bestIndex = i;
                    bestScore = score;
                }

                if (bestIndex < 0)
                {
                    break;
                }

                var best = _gridSlots[bestIndex];
                _accepted.Add(GetGridKey(best.Column, best.Row));
                _slots.Add(best.Position);
                minimumColumn = Mathf.Min(minimumColumn, best.Column);
                maximumColumn = Mathf.Max(maximumColumn, best.Column);
                minimumRow = Mathf.Min(minimumRow, best.Row);
                maximumRow = Mathf.Max(maximumRow, best.Row);
            }
        }

        private int CountConnectedNeighbours(GridSlot slot)
        {
            var count = 0;
            for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    if (columnOffset == 0 && rowOffset == 0)
                    {
                        continue;
                    }

                    var neighbourKey = GetGridKey(slot.Column + columnOffset, slot.Row + rowOffset);
                    if (!_accepted.Contains(neighbourKey))
                    {
                        continue;
                    }

                    var neighbour = _gridSlots[_gridSlotIndices[neighbourKey]];
                    if (!NavMesh.Raycast(slot.Position, neighbour.Position, out _, NavMesh.AllAreas))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private long GetGridKey(int column, int row)
        {
            return (long) column << 32 | (uint) row;
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
    }
}
