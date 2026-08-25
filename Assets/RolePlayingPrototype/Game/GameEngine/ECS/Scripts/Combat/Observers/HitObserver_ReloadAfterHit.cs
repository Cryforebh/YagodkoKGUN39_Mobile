using GameECS;

namespace Game.GameEngine.Ecs
{
    public class HitObserver_ReloadAfterHit : IEcsObserver<HitEvent>
    {
        private readonly EcsPool<CombatComponent> attackComponentPool;
        private readonly EcsPool<HitCountdown> reloadPool;

        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent @event)
        {
            ref var component = ref this.attackComponentPool.GetComponent(entity);
            this.reloadPool.SetComponent(entity, new HitCountdown
            {
                remainingTime = component.timeBetweenAttack
            });
        }
    }
}