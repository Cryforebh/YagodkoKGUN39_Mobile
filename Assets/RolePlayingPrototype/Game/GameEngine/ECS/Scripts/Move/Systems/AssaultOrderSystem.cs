using System.Collections.Generic;
using GameECS;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class AssaultOrderSystem : IEcsFixedUpdate
    {
        private const float NavigationSampleDistance = 8f;
        private const float PathRetryDelay = 1f;
        private const float MinimumProgressDistance = 0.15f;
        private const float StuckDuration = 0.65f;

        private EcsPool<AssaultOrderData> _assaultPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<HitPointsComponent> _hitPointsPool;
        private EcsPool<CommandRequest> _commandPool;
        private EcsPool<MoveToPositionData> _movePool;
        private INavigationPathService _navigation;

        [Inject]
        private void Construct(INavigationPathService navigation)
        {
            _navigation = navigation;
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _assaultPool, _transformPool, _hitPointsPool) ||
                _hitPointsPool.GetComponent(entity).Current <= 0)
            {
                return;
            }

            ref var order = ref _assaultPool.GetComponent(entity);
            var position = _transformPool.GetComponent(entity).Value.position;
            var destination = order.HasResolvedDestination ? order.ResolvedDestination : order.Destination;
            if (HorizontalDistance(position, destination) <= Mathf.Max(0.1f, order.ArrivalDistance))
            {
                return;
            }

            if (_commandPool.HasComponent(entity))
            {
                var command = _commandPool.GetComponent(entity);
                if (command.Type == CommandType.MOVE_TO_POSITION)
                {
                    RebuildStuckRoute(entity, position, ref order);
                }
                else
                {
                    order.IsTrackingProgress = false;
                }

                return;
            }

            order.IsTrackingProgress = false;

            if (Time.fixedTime < order.NextPathAttemptTime)
            {
                return;
            }

            TryIssueMoveCommand(entity, position, ref order);
        }

        private void RebuildStuckRoute(int entity, Vector3 position, ref AssaultOrderData order)
        {
            if (!_movePool.HasComponent(entity))
            {
                return;
            }

            var moveDestination = _movePool.GetComponent(entity).Destination;
            var distance = HorizontalDistance(position, moveDestination);
            if (!order.IsTrackingProgress || distance + MinimumProgressDistance < order.LastDistance)
            {
                order.IsTrackingProgress = true;
                order.LastDistance = distance;
                order.LastProgressTime = Time.fixedTime;
                return;
            }

            var delay = StuckDuration + entity % 5 * 0.05f;
            if (Time.fixedTime - order.LastProgressTime < delay || Time.fixedTime < order.NextPathAttemptTime)
            {
                return;
            }

            order.IsTrackingProgress = false;
            TryIssueMoveCommand(entity, position, ref order);
        }

        private void TryIssueMoveCommand(int entity, Vector3 position, ref AssaultOrderData order)
        {
            if (!_navigation.TryBuildPath(position, order.Destination, NavigationSampleDistance, out var path) || !path.IsComplete)
            {
                order.NextPathAttemptTime = Time.fixedTime + PathRetryDelay;
                return;
            }

            order.ResolvedDestination = path.Destination;
            order.HasResolvedDestination = true;
            order.NextPathAttemptTime = 0f;
            order.IsTrackingProgress = false;

            if (HorizontalDistance(position, path.Destination) <= Mathf.Max(0.1f, order.ArrivalDistance))
            {
                return;
            }

            _commandPool.SetComponent(entity, new CommandRequest
            {
                Type = CommandType.MOVE_TO_POSITION,
                Status = CommandStatus.IDLE,
                Args = CreateMoveArguments(path)
            });
        }

        private static object CreateMoveArguments(NavigationPathResult path)
        {
            if (path.Corners == null || path.Corners.Length <= 2)
            {
                return path.Destination;
            }

            var waypoints = new List<Vector3>(path.Corners.Length - 2);
            for (var index = 1; index < path.Corners.Length - 1; index++)
            {
                waypoints.Add(path.Corners[index]);
            }

            return new MoveRouteCommand
            {
                Destination = path.Destination,
                Waypoints = waypoints
            };
        }

        private static float HorizontalDistance(Vector3 left, Vector3 right)
        {
            var offset = right - left;
            offset.y = 0f;
            return offset.magnitude;
        }
    }
}
