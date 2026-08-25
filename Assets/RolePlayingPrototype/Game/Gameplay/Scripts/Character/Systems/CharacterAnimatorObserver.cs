using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CharacterAnimatorObserver : IEcsObserver<AnimatorEvent>
    {
        private const string ATTACK_MESSAGE = "attack";

        private EcsPool<HitRequest> requestPool;
        private EcsPool<CombatComponent> attackComponentPool;
        private EcsEmitter<HitEvent> hitEmitter;

        void IEcsObserver<AnimatorEvent>.Handle(int entity, AnimatorEvent @event)
        {
            if (@event.message == ATTACK_MESSAGE)
            {
                Debug.Log("ATTACK!");
                this.Attack(entity);
            }
        }

        private void Attack(int entity)
        {
            if (this.requestPool == null)
            {
                Debug.LogError("RQ POOL NULL");
            }
            
            if (!this.requestPool.HasComponent(entity))
            {
                return;
            }

            ref var request = ref this.requestPool.GetComponent(entity);
            ref var component = ref this.attackComponentPool.GetComponent(entity);

            this.hitEmitter.SendEvent(entity, new HitEvent
            {
                targetId = request.targetId,
                damage = component.damage,
                damageType = component.damageType
            });
        }
    }
}
