using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitSystem_LookAtTarget : IEcsFixedUpdate
    {
        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsEmitter<SmoothRotateEvent> rotateEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.hitRequestPool.HasComponent(entity))
            {
                return;
            }

            ref var request = ref this.hitRequestPool.GetComponent(entity);

            ref var myTransform = ref this.transformPool.GetComponent(entity).value;
            ref var targetTransform = ref this.transformPool.GetComponent(request.targetId).value;
            var direction = (targetTransform.position - myTransform.position).normalized;
            
            this.rotateEmitter.SendEvent(entity, new SmoothRotateEvent
            {
                direction = direction
            });
        }
    }
}