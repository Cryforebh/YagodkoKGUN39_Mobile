using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class PatrolGroupState
    {
        public IReadOnlyList<Vector3> Points { get; }
        public IReadOnlyCollection<EntityHandle> Members => _members;
        public int CurrentPoint { get; private set; }
        public bool HasFormation => _formationPoint == CurrentPoint && _destinations.Count > 0;

        private readonly HashSet<EntityHandle> _members = new();
        private readonly HashSet<EntityHandle> _arrived = new();
        private readonly Dictionary<EntityHandle, Vector3> _destinations = new();
        private int _formationPoint = -1;

        public PatrolGroupState(IReadOnlyList<Vector3> points)
        {
            Points = points;
        }

        public void Add(EntityHandle entity)
        {
            _members.Add(entity);
        }

        public void Remove(EntityHandle entity)
        {
            _members.Remove(entity);
            _arrived.Remove(entity);
            _destinations.Remove(entity);
        }

        public void MarkArrived(EntityHandle entity)
        {
            if (_members.Contains(entity))
            {
                _arrived.Add(entity);
            }
        }

        public bool HasArrived(EntityHandle entity)
        {
            return _arrived.Contains(entity);
        }

        public void SetFormation(IReadOnlyDictionary<EntityHandle, Vector3> destinations)
        {
            _destinations.Clear();
            foreach (var destination in destinations)
            {
                _destinations.Add(destination.Key, destination.Value);
            }

            _formationPoint = CurrentPoint;
        }

        public bool TryGetDestination(EntityHandle entity, out Vector3 destination)
        {
            return _destinations.TryGetValue(entity, out destination);
        }

        public bool TryMoveNext()
        {
            if (_members.Count == 0 || _arrived.Count < _members.Count)
            {
                return false;
            }

            CurrentPoint = (CurrentPoint + 1) % Points.Count;
            _arrived.Clear();
            _destinations.Clear();
            _formationPoint = -1;
            return true;
        }
    }
}
