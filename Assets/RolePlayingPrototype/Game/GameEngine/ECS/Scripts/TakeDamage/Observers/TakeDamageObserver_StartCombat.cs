using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class TakeDamageObserver_StartCombat : IEcsObserver<TakeDamageEvent>
    {
        private readonly EcsPool<CombatComponent> _combatPool;
        private readonly EcsPool<CommandRequest> _commandPool;
        private readonly EcsPool<HitPointsComponent> _hitPointsPool;
        private readonly EcsPool<TeamComponent> _teamPool;
        private EcsWorld _world;

        void IEcsObserver<TakeDamageEvent>.Handle(int entity, TakeDamageEvent takeDamageEvent)
        {
            if (!_world.IsEntityExists(takeDamageEvent.Source) || !_combatPool.HasComponent(entity) || !_hitPointsPool.HasComponent(entity) || _hitPointsPool.GetComponent(entity).Current <= 0)
            {
                return;
            }

            if (!_teamPool.HasComponent(entity) || !_teamPool.HasComponent(takeDamageEvent.Source.Id) || _teamPool.GetComponent(entity).Value == _teamPool.GetComponent(takeDamageEvent.Source.Id).Value)
            {
                return;
            }

            _commandPool.SetComponent(entity, new CommandRequest
            {
                Type = CommandType.ATTACK_TARGET,
                Status = CommandStatus.IDLE,
                Args = takeDamageEvent.Source
            });
        }
    }
}
