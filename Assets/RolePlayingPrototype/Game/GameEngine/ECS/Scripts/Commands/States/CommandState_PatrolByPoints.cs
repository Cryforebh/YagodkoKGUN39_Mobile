using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_PatrolByPoints : CommandState
    {
        private const float STOPPING_DISTANCE = 0.35f;

        private EcsPool<TransformComponent> transformPool;
        private EcsPool<PatrolData> patrolPointsPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.PATROL_BY_POINTS;
        }
        
        public override void Enter(int entity, object args)
        {
            this.patrolPointsPool.SetComponent(entity, new PatrolData
            {
                points = (List<Vector3>) args,
                pointer = 0,
                stoppingDistance = STOPPING_DISTANCE
            });
        }

        public override void Exit(int entity)
        {
            this.patrolPointsPool.RemoveComponent(entity);
            this.moveToPositionPool.RemoveComponent(entity);
        }
    }
}