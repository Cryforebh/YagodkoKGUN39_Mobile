using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class TakeDamageObserver_DecrementHitPoints : IEcsObserver<TakeDamageEvent>
    {
        private readonly EcsPool<HitPointsComponent> _hitPointsPool;
        
        void IEcsObserver<TakeDamageEvent>.Handle(int entity, TakeDamageEvent takeDamageEvent)
        {
            if (!_hitPointsPool.HasComponent(entity))
            {
                return;
            }

            ref var hitPoints = ref _hitPointsPool.GetComponent(entity);
            hitPoints.Current -= takeDamageEvent.Damage;
        }
    }
}
