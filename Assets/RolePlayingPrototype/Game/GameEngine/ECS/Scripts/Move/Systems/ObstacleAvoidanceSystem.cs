using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class ObstacleAvoidanceSystem : IEcsFixedUpdate
    {
        private const float DetectionDistance = 1.5f;
        private const float SteeringAngle = 45f;
        private const float MinRadius = 0.2f;
        private const float Height = 0.5f;

        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private EcsPool<MoveStepData> _moveStepPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<GameObjectComponent> _gameObjectPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _moveStepPool, _transformPool, _gameObjectPool))
            {
                return;
            }

            ref var step = ref _moveStepPool.GetComponent(entity);
            var direction = Vector3.ProjectOnPlane(step.Direction, Vector3.up).normalized;
            if (direction == Vector3.zero)
            {
                return;
            }

            ref var transformComponent = ref _transformPool.GetComponent(entity);
            var owner = _gameObjectPool.GetComponent(entity).Value;
            var origin = transformComponent.Value.position + Vector3.up * Height;
            var radius = Mathf.Max(MinRadius, transformComponent.Radius * 0.75f);
            if (IsClear(owner, origin, radius, direction))
            {
                return;
            }

            var preferLeft = entity % 2 == 0;
            var escapeRadius = Mathf.Max(MinRadius * 0.5f, radius * 0.35f);
            for (var turn = 1; turn <= 3; turn++)
            {
                var preferredAngle = (preferLeft ? -1f : 1f) * SteeringAngle * turn;
                var preferredDirection = Quaternion.AngleAxis(preferredAngle, Vector3.up) * direction;
                if (IsClear(owner, origin, escapeRadius, preferredDirection))
                {
                    step.Direction = preferredDirection;
                    return;
                }

                var alternativeDirection = Quaternion.AngleAxis(-preferredAngle, Vector3.up) * direction;
                if (IsClear(owner, origin, escapeRadius, alternativeDirection))
                {
                    step.Direction = alternativeDirection;
                    return;
                }
            }

            var reverseDirection = -direction;
            step.Direction = IsClear(owner, origin, escapeRadius, reverseDirection) ? reverseDirection : Vector3.zero;
        }

        private bool IsClear(GameObject owner, Vector3 origin, float radius, Vector3 direction)
        {
            var count = Physics.SphereCastNonAlloc(origin, radius, direction, _hits, DetectionDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < count; i++)
            {
                var collider = _hits[i].collider;
                if (collider == null || collider.transform.IsChildOf(owner.transform))
                {
                    continue;
                }

                if (collider.attachedRigidbody == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
