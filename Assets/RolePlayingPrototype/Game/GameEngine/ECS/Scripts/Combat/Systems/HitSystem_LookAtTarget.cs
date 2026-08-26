using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitSystem_LookAtTarget : IEcsFixedUpdate
    {
        private EcsPool<HitRequest> _hitRequestPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsEmitter<SmoothRotateEvent> _rotateEmitter;
        private EcsWorld _world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _hitRequestPool, _transformPool))
            {
                return;
            }

            ref var request = ref _hitRequestPool.GetComponent(entity);
            if (!_world.IsEntityExists(request.Target) || !_transformPool.HasComponent(request.Target.Id))
            {
                _hitRequestPool.RemoveComponent(entity);
                return;
            }

            ref var myTransform = ref _transformPool.GetComponent(entity).Value;
            ref var targetTransform = ref _transformPool.GetComponent(request.Target.Id).Value;
            var direction = (targetTransform.position - myTransform.position).normalized;
            
            _rotateEmitter.SendEvent(entity, new SmoothRotateEvent
            {
                Direction = direction
            });
        }
    }
}
