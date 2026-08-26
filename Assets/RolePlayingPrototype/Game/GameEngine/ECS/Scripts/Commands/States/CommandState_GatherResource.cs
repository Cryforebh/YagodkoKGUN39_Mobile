using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_GatherResource : CommandState
    {
        private EcsPool<GatherTarget> _targetResourcePool;
        private EcsPool<GatherState> _gatherStatePool;
        private EcsPool<GatherDuration> _gatherDurationPool;
        
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<MoveToPositionData> _moveToPositionPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.GATHER_RESOURCE;
        }

        public override void Enter(int entity, object args)
        {
            _targetResourcePool.SetComponent(entity, new GatherTarget
            {
                Target = (EntityHandle) args
            });
            _gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_RESOURCE);
        }

        public override void Update(int entity)
        {
            if (!_targetResourcePool.HasComponent(entity))
            {
                this.Complete(entity);
            }
        }

        public override void Exit(int entity)
        {
            _targetResourcePool.RemoveComponent(entity);
            _gatherStatePool.RemoveComponent(entity);
            _gatherDurationPool.RemoveComponent(entity);
            _moveToPositionPool.RemoveComponent(entity);
        }
    }
}
