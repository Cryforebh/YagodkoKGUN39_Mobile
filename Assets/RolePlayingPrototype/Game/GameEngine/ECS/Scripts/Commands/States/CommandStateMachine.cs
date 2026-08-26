using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandStateMachine : IEcsFixedUpdate, IEcsInjectable
    {
        private EcsPool<CommandRequest> _commandPool;

        private readonly CommandState[] _states;
        
        private bool _isEntered;
        private CommandRequest _command;

        public CommandStateMachine(params CommandState[] states)
        {
            _states = states;
        }

        public void Inject(EcsWorld world)
        {
            foreach (var state in _states)
            {
                world.Inject(state);
            }
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_commandPool.HasComponent(entity))
            {
                this.Exit(entity);
                return;   
            }

            ref var command = ref _commandPool.GetComponent(entity);
            if (!command.Equals(_command))
            {
                this.Exit(entity);
                _command = command;
            }
            
            if (command.Status is CommandStatus.COMPLETE or CommandStatus.FAIL)
            {
                this.Exit(entity);
                _commandPool.RemoveComponent(entity);
                return;
            }

            command.Status = CommandStatus.PLAYING;
            _command.Status = CommandStatus.PLAYING;

            this.Enter(entity);
            this.Update(entity);
        }


        private void Enter(int entity)
        {
            if (_isEntered)
            {
                return;
            }
            
            for (int i = 0, count = _states.Length; i < count; i++)
            {
                var state = _states[i];
                if (state.MatchesType(_command.Type))
                {
                    state.Enter(entity, _command.Args);
                }
            }

            _isEntered = true;
        }

        private void Exit(int entity)
        {
            if (!_isEntered)
            {
                return;
            }

            for (int i = 0, count = _states.Length; i < count; i++)
            {
                var state = _states[i];
                if (state.MatchesType(_command.Type))
                {
                    state.Exit(entity);
                }
            }

            _isEntered = false;
            _command = default;
        }

        private void Update(int entity)
        {
            for (int i = 0, count = _states.Length; i < count; i++)
            {
                var state = _states[i];
                if (state.MatchesType(_command.Type))
                {
                    state.Update(entity);
                }
            }
        }
    }
}
