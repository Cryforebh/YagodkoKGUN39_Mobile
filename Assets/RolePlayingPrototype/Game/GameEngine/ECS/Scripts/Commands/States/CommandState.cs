using GameECS;

namespace Game.GameEngine.Ecs
{
    public abstract class CommandState
    {
        private EcsPool<CommandRequest> _commandPool;

        public abstract bool MatchesType(CommandType type);

        public virtual void Enter(int entity, object args)
        {
        }

        public virtual void Update(int entity)
        {
        }

        public virtual void Exit(int entity)
        {
        }

        protected void Fail(int entity)
        {
            if (_commandPool.HasComponent(entity))
            {
                ref var command = ref _commandPool.GetComponent(entity);
                command.Status = CommandStatus.FAIL;
            }
        }

        protected void Complete(int entity)
        {
            if (_commandPool.HasComponent(entity))
            {
                ref var command = ref _commandPool.GetComponent(entity);
                command.Status = CommandStatus.COMPLETE;
            }
        }
    }
}