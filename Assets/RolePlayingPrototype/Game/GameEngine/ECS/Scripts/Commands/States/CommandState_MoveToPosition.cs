using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_MoveToPosition : CommandState
    {
        private const float STOPPING_DISTANCE = 0.2f;
        private const float WAYPOINT_STOPPING_DISTANCE = 0.6f;

        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<MoveRouteData> _moveRoutePool;
        private EcsPool<TransformComponent> _transformPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.MOVE_TO_POSITION;
        }

        public override void Enter(int entity, object args)
        {
            if (args is MoveRouteCommand routeCommand)
            {
                _moveRoutePool.SetComponent(entity, new MoveRouteData
                {
                    Destination = routeCommand.Destination,
                    Waypoints = routeCommand.Waypoints,
                    Pointer = 0
                });
                SetRouteDestination(entity);
                return;
            }

            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = (Vector3) args,
                StoppingDistance = STOPPING_DISTANCE
            });
        }

        public override void Update(int entity)
        {
            ref var moveData = ref _moveToPositionPool.GetComponent(entity);
            if (moveData.IsReached)
            {
                if (!_moveRoutePool.HasComponent(entity))
                {
                    this.Complete(entity);
                    return;
                }

                ref var route = ref _moveRoutePool.GetComponent(entity);
                if (route.Pointer < route.Waypoints.Count)
                {
                    route.Pointer++;
                    SetRouteDestination(entity);
                }
                else
                {
                    this.Complete(entity);
                }
            }
        }

        public override void Exit(int entity)
        {
            _moveToPositionPool.RemoveComponent(entity);
            _moveRoutePool.RemoveComponent(entity);
        }

        private void SetRouteDestination(int entity)
        {
            ref var route = ref _moveRoutePool.GetComponent(entity);
            var isFinalDestination = route.Pointer >= route.Waypoints.Count;
            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = isFinalDestination ? route.Destination : route.Waypoints[route.Pointer],
                StoppingDistance = isFinalDestination ? STOPPING_DISTANCE : WAYPOINT_STOPPING_DISTANCE
            });
        }
    }
}
