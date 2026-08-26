using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class UnitSeparationSystem : IEcsFixedUpdate
    {
        private const float SeparationPadding = 0.2f;
        private const float SeparationWeight = 1.4f;
        private readonly List<EntityHandle> _entities = new();
        private EcsPool<MoveStepData> _moveStepPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<RigidbodyComponent> _rigidbodyPool;
        private EcsPool<TeamComponent> _teamPool;
        private EcsWorld _world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _moveStepPool, _transformPool, _rigidbodyPool, _teamPool))
            {
                return;
            }

            ref var step = ref _moveStepPool.GetComponent(entity);
            var transform = _transformPool.GetComponent(entity);
            var team = _teamPool.GetComponent(entity).Value;
            var desiredDirection = Vector3.ProjectOnPlane(step.Direction, Vector3.up).normalized;
            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var separation = Vector3.zero;
            _world.GetActiveEntities(_entities);

            for (var i = 0; i < _entities.Count; i++)
            {
                var other = _entities[i];
                if (other.Id == entity || !EcsFilter.Matches(other.Id, _transformPool, _rigidbodyPool, _teamPool))
                {
                    continue;
                }

                if (_teamPool.GetComponent(other.Id).Value != team)
                {
                    continue;
                }

                var otherTransform = _transformPool.GetComponent(other.Id);
                var difference = Vector3.ProjectOnPlane(transform.Value.position - otherTransform.Value.position, Vector3.up);
                var minimumDistance = transform.Radius + otherTransform.Radius + SeparationPadding;
                if (difference.sqrMagnitude >= minimumDistance * minimumDistance)
                {
                    continue;
                }

                if (difference.sqrMagnitude <= 0.0001f)
                {
                    var firstEntity = Mathf.Min(entity, other.Id);
                    var secondEntity = Mathf.Max(entity, other.Id);
                    var angle = ((firstEntity * 397) ^ (secondEntity * 613)) % 360;
                    difference = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * 0.01f;
                    if (entity > other.Id)
                    {
                        difference = -difference;
                    }
                }

                var strength = 1f - difference.magnitude / minimumDistance;
                separation += difference.normalized * strength;
                var side = entity < other.Id ? -1f : 1f;
                separation += Vector3.Cross(Vector3.up, desiredDirection).normalized * strength * side * 0.35f;
            }

            if (separation.sqrMagnitude > 0.0001f)
            {
                var backwardStrength = Vector3.Dot(separation, desiredDirection);
                if (backwardStrength < 0f)
                {
                    separation -= desiredDirection * backwardStrength;
                }

                separation = Vector3.ClampMagnitude(separation, 1f);
                var steeredDirection = Vector3.ProjectOnPlane(desiredDirection + separation * SeparationWeight, Vector3.up).normalized;
                step.Direction = Vector3.Dot(steeredDirection, desiredDirection) > 0.05f ? steeredDirection : desiredDirection;
            }
        }
    }
}
