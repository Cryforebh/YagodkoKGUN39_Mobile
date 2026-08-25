using GameECS;
using SampleProject.Base;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_GatherResource : CommandState
    {
        private EcsPool<GatherTarget> targetResourcePool;
        private EcsPool<GatherState> gatherStatePool;
        private EcsPool<GatherDuration> gatherDurationPool;
        
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.GATHER_RESOURCE;
        }

        public override void Enter(int entity, object args)
        {
            this.targetResourcePool.SetComponent(entity, new GatherTarget
            {
                targetId = ((Entity) args).Id
            });
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_RESOURCE);
        }

        public override void Update(int entity)
        {
            if (!this.targetResourcePool.HasComponent(entity))
            {
                this.Complete(entity);
            }
        }

        public override void Exit(int entity)
        {
            this.targetResourcePool.RemoveComponent(entity);
            this.gatherStatePool.RemoveComponent(entity);
            this.gatherDurationPool.RemoveComponent(entity);
            this.moveToPositionPool.RemoveComponent(entity);
        }
    }
}
