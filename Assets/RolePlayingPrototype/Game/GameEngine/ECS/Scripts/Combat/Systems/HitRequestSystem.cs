using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class HitRequestSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<HitRequest> _requestPool;
        private readonly EcsPool<CombatComponent> _combatPool;
        private readonly EcsPool<HitCountdown> _reloadPool;
        private readonly EcsPool<HitDuration> _durationPool;
        
        private readonly EcsEmitter<TakeDamageEvent> _takeDamageEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_requestPool.HasComponent(entity))
            {
                _reloadPool.RemoveComponent(entity);
                _durationPool.RemoveComponent(entity);
                return;
            }

            if (!_combatPool.HasComponent(entity))
            {
                _requestPool.RemoveComponent(entity);
                return;
            }

            if (_reloadPool.HasComponent(entity) || _durationPool.HasComponent(entity))
            {
                return;
            }

            //Start attack:
            this.SetDuration(entity);
        }

        private void SetDuration(int entity)
        {
            ref var hitComponent = ref _combatPool.GetComponent(entity);
            _durationPool.SetComponent(entity, new HitDuration
            {
                RemainingTime = hitComponent.AnimationTime
            });
        }
    }
}
