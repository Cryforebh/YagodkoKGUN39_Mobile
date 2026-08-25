using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class HitObserver_DealMeleeDamage : IEcsObserver<HitEvent>
    {
        private readonly EcsEmitter<TakeDamageEvent> takeDamageEmitter;
        
        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent @event)
        {
            if (@event.damageType != DamageType.MELEE)
            {
                return;
            }

            this.takeDamageEmitter.SendEvent(@event.targetId, new TakeDamageEvent
            {
                sourceId = entity,
                damage = @event.damage,
                damageType = @event.damageType
            });
        }
    }
}

