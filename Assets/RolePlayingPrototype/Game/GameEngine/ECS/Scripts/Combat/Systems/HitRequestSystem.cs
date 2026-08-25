using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class HitRequestSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<HitRequest> requestPool;
        private readonly EcsPool<CombatComponent> combatPool;
        private readonly EcsPool<HitCountdown> reloadPool;
        private readonly EcsPool<HitDuration> durationPool;
        
        private readonly EcsEmitter<TakeDamageEvent> takeDamageEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.requestPool.HasComponent(entity))
            {
                this.reloadPool.RemoveComponent(entity);
                this.durationPool.RemoveComponent(entity);
                return;
            }

            if (this.reloadPool.HasComponent(entity) || this.durationPool.HasComponent(entity))
            {
                return;
            }

            //Start attack:
            this.SetDuration(entity);
        }

        private void SetDuration(int entity)
        {
            ref var hitComponent = ref this.combatPool.GetComponent(entity);
            this.durationPool.SetComponent(entity, new HitDuration
            {
                remainingTime = hitComponent.animationTime
            });
        }
    }
}