using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UniRx;
using UnityEngine;

namespace SampleProject
{
    public interface IPatrolRouteEditor
    {
        bool IsEditing { get; }
        int PointCount { get; }
        bool CanUndoPoint { get; }
        IReadOnlyList<Vector3> Points { get; }
        event Action Changed;
        void Begin();
        void AddPoint(Vector3 point);
        void UndoOrExit();
        void ClearPoints();
        void Apply();
    }

    public sealed class PatrolRouteEditor : IPatrolRouteEditor, IDisposable
    {
        public const int MaximumPointCount = 6;

        private readonly List<Vector3> _points = new(MaximumPointCount);
        private readonly List<EntityHandle> _units = new();
        private readonly CompositeDisposable _subscriptions = new();
        private readonly IUnitSelectionService _selection;
        private readonly IEntityCommandService _commands;
        private readonly EcsWorld _world;
        private int _addedPointCount;

        public bool IsEditing { get; private set; }
        public int PointCount => _points.Count;
        public bool CanUndoPoint => _addedPointCount > 0;
        public IReadOnlyList<Vector3> Points => _points;
        public event Action Changed;

        public PatrolRouteEditor(IUnitSelectionService selection, IEntityCommandService commands, EcsWorld world)
        {
            _selection = selection;
            _commands = commands;
            _world = world;
            _selection.Selected.ObserveRemove().Subscribe(_ => CloseEditorIfActive()).AddTo(_subscriptions);
        }

        public void Begin()
        {
            if (IsEditing || _selection.Selected.Count == 0)
            {
                return;
            }

            _units.Clear();
            for (var i = 0; i < _selection.Selected.Count; i++)
            {
                _units.Add(_selection.Selected[i]);
            }

            _points.Clear();
            LoadCommonRoute();
            _addedPointCount = 0;
            IsEditing = true;
            Changed?.Invoke();
        }

        public void AddPoint(Vector3 point)
        {
            if (!IsEditing || _points.Count >= MaximumPointCount)
            {
                return;
            }

            _points.Add(point);
            _addedPointCount++;
            Changed?.Invoke();
        }

        public void UndoOrExit()
        {
            if (!IsEditing)
            {
                return;
            }

            if (_addedPointCount == 0)
            {
                CloseEditor();
                return;
            }

            _points.RemoveAt(_points.Count - 1);
            _addedPointCount = 0;
            Changed?.Invoke();
        }

        public void ClearPoints()
        {
            if (!IsEditing || _points.Count == 0)
            {
                return;
            }

            _points.Clear();
            _addedPointCount = 0;
            Changed?.Invoke();
        }

        public void Apply()
        {
            if (!IsEditing)
            {
                return;
            }

            var group = _points.Count > 0 ? new PatrolGroupState(new List<Vector3>(_points)) : null;
            for (var i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                if (!_world.IsEntityExists(unit))
                {
                    continue;
                }

                if (_points.Count == 0)
                {
                    _commands.Stop(unit);
                }
                else
                {
                    _commands.Patrol(unit, group.Points, group);
                }
            }

            CloseEditor();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            Changed = null;
        }

        private void CloseEditorIfActive()
        {
            if (!IsEditing)
            {
                return;
            }

            CloseEditor();
        }

        private void CloseEditor()
        {
            _points.Clear();
            _units.Clear();
            _addedPointCount = 0;
            IsEditing = false;
            Changed?.Invoke();
        }

        private void LoadCommonRoute()
        {
            if (_units.Count == 0 || !_world.HasComponent<PatrolRouteComponent>(_units[0].Id))
            {
                return;
            }

            var firstRoute = _world.GetComponent<PatrolRouteComponent>(_units[0].Id).Points;
            if (firstRoute == null || firstRoute.Count == 0)
            {
                return;
            }

            for (var i = 1; i < _units.Count; i++)
            {
                if (!_world.HasComponent<PatrolRouteComponent>(_units[i].Id) || !RoutesMatch(firstRoute, _world.GetComponent<PatrolRouteComponent>(_units[i].Id).Points))
                {
                    return;
                }
            }

            _points.AddRange(firstRoute);
        }

        private static bool RoutesMatch(IReadOnlyList<Vector3> first, IReadOnlyList<Vector3> second)
        {
            if (second == null || first.Count != second.Count)
            {
                return false;
            }

            for (var i = 0; i < first.Count; i++)
            {
                if ((first[i] - second[i]).sqrMagnitude > 0.0001f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
