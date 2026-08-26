using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class HitObserver_DealMeleeDamage : IEcsObserver<HitEvent>
    {
        private readonly EcsEmitter<TakeDamageEvent> _takeDamageEmitter;
        private EcsWorld _world;
        
        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent @event)
        {
            if (@event.DamageType != DamageType.MELEE)
            {
                return;
            }

            if (!_world.IsEntityExists(@event.Target))
            {
                return;
            }

            _takeDamageEmitter.SendEvent(@event.Target.Id, new TakeDamageEvent
            {
                Source = _world.GetEntityHandle(entity),
                Damage = @event.Damage,
                DamageType = @event.DamageType
            });
        }
    }
}
