using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class TakeDamageObserver_DecrementHitPoints : IEcsObserver<TakeDamageEvent>
    {
        private readonly EcsPool<HitPointsComponent> hitPointsPool;
        
        void IEcsObserver<TakeDamageEvent>.Handle(int entity, TakeDamageEvent takeDamageEvent)
        {
            ref var hitPoints = ref this.hitPointsPool.GetComponent(entity);
            hitPoints.current -= takeDamageEvent.damage;
        }
    }
}