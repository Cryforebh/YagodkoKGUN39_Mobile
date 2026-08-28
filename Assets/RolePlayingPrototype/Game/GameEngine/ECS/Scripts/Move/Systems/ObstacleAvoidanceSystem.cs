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
        private EcsPool<MoveToPositionData> _moveToPositionPool;
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

            var preferredDirection = Vector3.ProjectOnPlane(step.PreferredDirection, Vector3.up).normalized;
            if (preferredDirection == Vector3.zero)
            {
                preferredDirection = direction;
            }

            ref var transformComponent = ref _transformPool.GetComponent(entity);
            var owner = _gameObjectPool.GetComponent(entity).Value;
            var origin = transformComponent.Value.position + Vector3.up * Height;
            var radius = Mathf.Max(MinRadius, transformComponent.Radius * 0.75f);
            var detectionDistance = GetDetectionDistance(entity, transformComponent.Value.position);
            if (detectionDistance <= 0f || IsClear(owner, origin, radius, direction, detectionDistance))
            {
                return;
            }

            if (Vector3.Dot(direction, preferredDirection) < 0.999f && IsClear(owner, origin, radius, preferredDirection, detectionDistance))
            {
                step.Direction = preferredDirection;
                return;
            }

            var preferLeft = entity % 2 == 0;
            var escapeRadius = Mathf.Max(MinRadius * 0.5f, radius * 0.35f);
            for (var turn = 1; turn <= 3; turn++)
            {
                var preferredAngle = (preferLeft ? -1f : 1f) * SteeringAngle * turn;
                var firstDirection = Quaternion.AngleAxis(preferredAngle, Vector3.up) * preferredDirection;
                if (IsClear(owner, origin, escapeRadius, firstDirection, detectionDistance))
                {
                    step.Direction = firstDirection;
                    return;
                }

                var alternativeDirection = Quaternion.AngleAxis(-preferredAngle, Vector3.up) * preferredDirection;
                if (IsClear(owner, origin, escapeRadius, alternativeDirection, detectionDistance))
                {
                    step.Direction = alternativeDirection;
                    return;
                }
            }

            var reverseDirection = -preferredDirection;
            step.Direction = IsClear(owner, origin, escapeRadius, reverseDirection, detectionDistance) ? reverseDirection : Vector3.zero;
        }

        private float GetDetectionDistance(int entity, Vector3 position)
        {
            if (!_moveToPositionPool.HasComponent(entity))
            {
                return DetectionDistance;
            }

            ref var movement = ref _moveToPositionPool.GetComponent(entity);
            var offset = Vector3.ProjectOnPlane(movement.Destination - position, Vector3.up);
            return Mathf.Min(DetectionDistance, Mathf.Max(0f, offset.magnitude - movement.StoppingDistance));
        }

        private bool IsClear(GameObject owner, Vector3 origin, float radius, Vector3 direction, float distance)
        {
            var count = Physics.SphereCastNonAlloc(origin, radius, direction, _hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
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
