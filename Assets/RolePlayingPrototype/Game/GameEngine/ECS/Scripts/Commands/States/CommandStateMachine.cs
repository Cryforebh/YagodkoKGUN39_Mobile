using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandStateMachine : IEcsFixedUpdate, IEcsInjectable
    {
        private EcsPool<CommandRequest> commandPool;

        private readonly CommandState[] states;
        
        private bool isEntered;
        private CommandRequest command;

        public CommandStateMachine(params CommandState[] states)
        {
            this.states = states;
        }

        public void Inject(EcsWorld world)
        {
            foreach (var state in states)
            {
                world.Inject(state);
            }
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.commandPool.HasComponent(entity))
            {
                this.Exit(entity);
                return;   
            }

            ref var command = ref this.commandPool.GetComponent(entity);
            if (!command.Equals(this.command))
            {
                this.Exit(entity);
                this.command = command;
            }
            
            if (this.command.status is CommandStatus.COMPLETE or CommandStatus.FAIL)
            {
                this.Exit(entity);
                return;
            }

            this.command.status = CommandStatus.PLAYING;

            this.Enter(entity);
            this.Update(entity);
        }


        private void Enter(int entity)
        {
            if (this.isEntered)
            {
                return;
            }
            
            for (int i = 0, count = this.states.Length; i < count; i++)
            {
                var state = this.states[i];
                if (state.MatchesType(this.command.type))
                {
                    state.Enter(entity, this.command.args);
                }
            }

            this.isEntered = true;
        }

        private void Exit(int entity)
        {
            if (!this.isEntered)
            {
                return;
            }

            for (int i = 0, count = this.states.Length; i < count; i++)
            {
                var state = this.states[i];
                if (state.MatchesType(this.command.type))
                {
                    state.Exit(entity);
                }
            }

            this.isEntered = false;
            this.command = default;
        }

        private void Update(int entity)
        {
            for (int i = 0, count = this.states.Length; i < count; i++)
            {
                var state = this.states[i];
                if (state.MatchesType(this.command.type))
                {
                    state.Update(entity);
                }
            }
        }
    }
}
