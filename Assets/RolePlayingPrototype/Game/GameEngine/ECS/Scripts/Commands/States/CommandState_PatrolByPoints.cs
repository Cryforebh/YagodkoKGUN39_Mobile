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
        private EcsPool<MoveToPositionData> _moveToPositionPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.PATROL_BY_POINTS;
        }
        
        public override void Enter(int entity, object args)
        {
            _patrolPointsPool.SetComponent(entity, new PatrolData
            {
                Points = (List<Vector3>) args,
                Pointer = 0,
                StoppingDistance = STOPPING_DISTANCE
            });
        }

        public override void Exit(int entity)
        {
            _patrolPointsPool.RemoveComponent(entity);
            _moveToPositionPool.RemoveComponent(entity);
        }
    }
}