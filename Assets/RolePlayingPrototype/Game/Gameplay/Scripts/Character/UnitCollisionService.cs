using System.Collections.Generic;
using UnityEngine;

namespace SampleProject
{
    public interface IUnitCollisionService
    {
        void Register(Collider[] colliders);
    }

    public sealed class UnitCollisionService : IUnitCollisionService
    {
        private readonly HashSet<Collider> _colliders = new();

        public void Register(Collider[] colliders)
        {
            _colliders.RemoveWhere(collider => collider == null);
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider == null || _colliders.Contains(collider))
                {
                    continue;
                }

                foreach (var registeredCollider in _colliders)
                {
                    Physics.IgnoreCollision(collider, registeredCollider, true);
                }

                _colliders.Add(collider);
            }
        }
    }
}
