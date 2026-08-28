using System;
using System.Collections.Generic;
using Entities;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace SampleProject
{
    public sealed class PatrolGroupAuthoring : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Scene units that use this shared patrol route.")]
        private CharacterEntity[] _members = Array.Empty<CharacterEntity>();

        [SerializeField]
        [Tooltip("Shared world-space patrol route. Maximum 6 points.")]
        private Vector3[] _points = Array.Empty<Vector3>();

        private List<Vector3> _runtimePoints;
        private PatrolGroupState _runtimeGroup;

        public IReadOnlyList<CharacterEntity> Members => _members;
        public IReadOnlyList<Vector3> Points => _points;

        public bool TryJoin(EntityHandle entity, out PatrolGroupState group, out List<Vector3> points)
        {
            group = null;
            points = null;
            if (_points == null || _points.Length == 0)
            {
                return false;
            }

            if (_runtimeGroup == null)
            {
                _runtimePoints = new List<Vector3>(_points);
                _runtimeGroup = new PatrolGroupState(_runtimePoints);
            }

            _runtimeGroup.Add(entity);
            group = _runtimeGroup;
            points = _runtimePoints;
            return true;
        }

        private void OnValidate()
        {
            if (_points != null && _points.Length > PatrolRouteEditor.MaximumPointCount)
            {
                Array.Resize(ref _points, PatrolRouteEditor.MaximumPointCount);
            }
        }
    }
}
