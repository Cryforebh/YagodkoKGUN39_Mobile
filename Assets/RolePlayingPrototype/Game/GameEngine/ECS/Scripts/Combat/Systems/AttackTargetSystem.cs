using GameECS;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackTargetSystem : IEcsFixedUpdate
    {
        private const float NavigationSampleDistance = 2f;
        private const float RepathInterval = 0.5f;
        private const float TargetMoveThreshold = 0.5f;
        private const float WaypointStoppingDistance = 0.6f;

        private EcsPool<AttackTarget> _targetPool;
        private EcsPool<HitRequest> _hitRequestPool;
        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<AttackNavigationData> _navigationPool;

        private EcsPool<CombatComponent> _combatPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsWorld _world;

        private INavigationPathService _navigation;

        [Inject]
        private void Construct(INavigationPathService navigation)
        {
            _navigation = navigation;
        }
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _targetPool, _transformPool, _combatPool))
            {
                return;
            }

            ref var target = ref _targetPool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_transformPool.HasComponent(target.Id))
            {
                _targetPool.RemoveComponent(entity);
                _hitRequestPool.RemoveComponent(entity);
                _moveToPositionPool.RemoveComponent(entity);
                _navigationPool.RemoveComponent(entity);
                return;
            }
            
            var myPosition = _transformPool.GetComponent(entity).Value.position;
            var targetPosition = _transformPool.GetComponent(target.Id).Value.position;
            ref var minDistance = ref _combatPool.GetComponent(entity).MinDistance;

            if (Vector3.Distance(myPosition, targetPosition) <= minDistance && HasDirectPath(myPosition, targetPosition))
            {
                //Attack target:
                _moveToPositionPool.RemoveComponent(entity);
                _navigationPool.RemoveComponent(entity);
                _hitRequestPool.SetComponent(entity, new HitRequest
                {
                    Target = target
                });
            }
            else
            {
                //Move to target:
                _hitRequestPool.RemoveComponent(entity);
                UpdateNavigation(entity, target, myPosition, targetPosition, minDistance);
            }
        }

        private void UpdateNavigation(int entity, EntityHandle target, Vector3 position, Vector3 targetPosition, float minDistance)
        {
            var shouldRepath = !_navigationPool.HasComponent(entity);
            if (!shouldRepath)
            {
                ref var navigation = ref _navigationPool.GetComponent(entity);
                var targetOffset = targetPosition - navigation.TargetPosition;
                shouldRepath = navigation.Target != target || Time.fixedTime >= navigation.NextRepathTime &&
                               (targetOffset.sqrMagnitude >= TargetMoveThreshold * TargetMoveThreshold || navigation.Pointer >= navigation.Corners.Length);
            }

            if (shouldRepath && !BuildPath(entity, target, position, targetPosition))
            {
                _moveToPositionPool.RemoveComponent(entity);
                return;
            }

            if (!_navigationPool.HasComponent(entity))
            {
                _moveToPositionPool.RemoveComponent(entity);
                return;
            }

            ref var route = ref _navigationPool.GetComponent(entity);
            while (route.Pointer < route.Corners.Length - 1 && HorizontalDistance(position, route.Corners[route.Pointer]) <= WaypointStoppingDistance)
            {
                route.Pointer++;
            }

            if (route.Pointer >= route.Corners.Length)
            {
                _moveToPositionPool.RemoveComponent(entity);
                return;
            }

            var isLastCorner = route.Pointer == route.Corners.Length - 1;
            var destination = isLastCorner && route.IsComplete ? route.TargetPosition : route.Corners[route.Pointer];
            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = destination,
                StoppingDistance = isLastCorner ? Mathf.Max(0.1f, minDistance * 0.8f) : WaypointStoppingDistance
            });
        }

        private bool BuildPath(int entity, EntityHandle target, Vector3 position, Vector3 targetPosition)
        {
            if (!_navigation.TryBuildPath(position, targetPosition, NavigationSampleDistance, out var path))
            {
                _navigationPool.RemoveComponent(entity);
                return false;
            }

            _navigationPool.SetComponent(entity, new AttackNavigationData
            {
                Target = target,
                TargetPosition = targetPosition,
                Corners = path.Corners,
                Pointer = 1,
                NextRepathTime = Time.fixedTime + RepathInterval + entity % 7 * 0.03f,
                IsComplete = path.IsComplete
            });
            return true;
        }

        private bool HasDirectPath(Vector3 position, Vector3 targetPosition)
        {
            return _navigation.HasDirectPath(position, targetPosition, NavigationSampleDistance);
        }

        private float HorizontalDistance(Vector3 left, Vector3 right)
        {
            var offset = right - left;
            offset.y = 0f;
            return offset.magnitude;
        }
    }
}
