using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CharacterAnimatorObserver : IEcsObserver<AnimatorEvent>
    {
        private const string ATTACK_MESSAGE = "attack";

        private EcsPool<HitRequest> _requestPool;
        private EcsPool<CombatComponent> _attackComponentPool;
        private EcsEmitter<HitEvent> _hitEmitter;

        void IEcsObserver<AnimatorEvent>.Handle(int entity, AnimatorEvent @event)
        {
            if (@event.Message == ATTACK_MESSAGE)
            {
                Attack(entity);
            }
        }

        private void Attack(int entity)
        {
            if (_requestPool == null)
            {
                Debug.LogError("RQ POOL NULL");
            }
            
            if (!_requestPool.HasComponent(entity))
            {
                return;
            }

            ref var request = ref _requestPool.GetComponent(entity);
            ref var component = ref _attackComponentPool.GetComponent(entity);

            _hitEmitter.SendEvent(entity, new HitEvent
            {
                Target = request.Target,
                Damage = component.Damage,
                DamageType = component.DamageType
            });
            Debug.Log("ATTACK!");
        }
    }
}
