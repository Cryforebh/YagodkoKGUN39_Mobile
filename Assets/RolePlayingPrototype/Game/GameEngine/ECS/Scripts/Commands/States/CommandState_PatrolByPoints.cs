using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_PatrolByPoints : CommandState
    {
        private const float STOPPING_DISTANCE = 0.35f;

        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<PatrolData> _patrolPointsPool;
        private EcsPool<PatrolRouteComponent> _patrolRoutePool;
        private EcsPool<PatrolNavigationData> _navigationPool;
        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsWorld _world;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.PATROL_BY_POINTS;
        }
        
        public override void Enter(int entity, object args)
        {
            ref var route = ref _patrolRoutePool.GetComponent(entity);
            if (route.Group == null)
            {
                route.Group = new PatrolGroupState(route.Points);
                route.Group.Add(_world.GetEntityHandle(entity));
            }

            _patrolPointsPool.SetComponent(entity, new PatrolData
            {
                Group = route.Group,
                TargetPoint = route.Group.CurrentPoint,
                StoppingDistance = STOPPING_DISTANCE
            });
        }

        public override void Exit(int entity)
        {
            _patrolPointsPool.RemoveComponent(entity);
            _navigationPool.RemoveComponent(entity);
            _moveToPositionPool.RemoveComponent(entity);
        }
    }
}
