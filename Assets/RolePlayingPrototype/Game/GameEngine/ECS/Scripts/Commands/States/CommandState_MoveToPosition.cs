using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_MoveToPosition : CommandState
    {
        private const float STOPPING_DISTANCE = 0.2f;

        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<TransformComponent> _transformPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.MOVE_TO_POSITION;
        }

        public override void Enter(int entity, object args)
        {
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
                this.Complete(entity);
            }
        }

        public override void Exit(int entity)
        {
            _moveToPositionPool.RemoveComponent(entity);
        }
    }
}