using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

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
        private const float FORMATION_SPACING = 1.5f;

        private readonly IUnitSelectionService _selection;
        private readonly IEntityCommandService _commands;
        private readonly EcsWorld _world;
        private readonly List<EntityHandle> _remainingUnits = new();
        private readonly List<Vector3> _formationSlots = new();

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
                var forwardOffset = ((rows - 1) * 0.5f - row) * FORMATION_SPACING;
                for (var column = 0; column < unitsInRow; column++)
                {
                    var rightOffset = (column - (unitsInRow - 1) * 0.5f) * FORMATION_SPACING;
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

            for (var i = 0; i < count; i++)
            {
                _commands.Move(_remainingUnits[i], _formationSlots[i]);
            }
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
