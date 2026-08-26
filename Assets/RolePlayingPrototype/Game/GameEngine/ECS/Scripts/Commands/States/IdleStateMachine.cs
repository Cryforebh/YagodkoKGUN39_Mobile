using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class IdleStateMachine : IEcsFixedUpdate, IEcsInjectable
    {
        private EcsPool<CommandRequest> _commandPool;

        private readonly IIdleState[] _states;
        private bool _isEntered;

        public IdleStateMachine(params IIdleState[] states)
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
            if (_commandPool.HasComponent(entity))
            {
                this.Exit(entity);
                return;
            }

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
                state.OnEnter(entity);
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
                state.OnExit(entity);
            }

            _isEntered = false;
        }

        private void Update(int entity)
        {
            for (int i = 0, count = _states.Length; i < count; i++)
            {
                var state = _states[i];
                state.OnUpdate(entity);
            }
        }
    }
}
