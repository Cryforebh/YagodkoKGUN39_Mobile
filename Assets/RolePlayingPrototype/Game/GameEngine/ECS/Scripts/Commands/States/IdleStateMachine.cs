using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class IdleStateMachine : IEcsFixedUpdate, IEcsInjectable
    {
        private EcsPool<CommandRequest> commandPool;

        private readonly IIdleState[] states;
        private bool isEntered;

        public IdleStateMachine(params IIdleState[] states)
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
            if (this.commandPool.HasComponent(entity))
            {
                this.Exit(entity);
                return;
            }

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
                state.OnEnter(entity);
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
                state.OnExit(entity);
            }

            this.isEntered = false;
        }

        private void Update(int entity)
        {
            for (int i = 0, count = this.states.Length; i < count; i++)
            {
                var state = this.states[i];
                state.OnUpdate(entity);
            }
        }
    }
}
