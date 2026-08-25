using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_MoveToPosition : CommandState
    {
        private const float STOPPING_DISTANCE = 0.2f;

        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.MOVE_TO_POSITION;
        }

        public override void Enter(int entity, object args)
        {
            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = (Vector3) args,
                stoppingDistance = STOPPING_DISTANCE
            });
        }

        public override void Update(int entity)
        {
            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (moveData.isReached)
            {
                this.Complete(entity);
            }
        }

        public override void Exit(int entity)
        {
            this.moveToPositionPool.RemoveComponent(entity);
        }
    }
}