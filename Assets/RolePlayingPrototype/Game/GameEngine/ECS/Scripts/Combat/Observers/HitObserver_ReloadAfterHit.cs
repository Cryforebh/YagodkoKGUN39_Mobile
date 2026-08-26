using GameECS;

namespace Game.GameEngine.Ecs
{
    public class HitObserver_ReloadAfterHit : IEcsObserver<HitEvent>
    {
        private readonly EcsPool<CombatComponent> _attackComponentPool;
        private readonly EcsPool<HitCountdown> _reloadPool;

        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent @event)
        {
            ref var component = ref _attackComponentPool.GetComponent(entity);
            _reloadPool.SetComponent(entity, new HitCountdown
            {
                RemainingTime = component.TimeBetweenAttack
            });
        }
    }
}