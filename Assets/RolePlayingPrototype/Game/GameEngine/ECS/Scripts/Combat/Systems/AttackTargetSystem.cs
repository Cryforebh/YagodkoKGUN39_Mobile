using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackTargetSystem : IEcsFixedUpdate
    {
        private EcsPool<AttackTarget> _targetPool;
        private EcsPool<HitRequest> _hitRequestPool;
        private EcsPool<MoveToPositionData> _moveToPositionPool;

        private EcsPool<CombatComponent> _combatPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsWorld _world;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _targetPool, _transformPool, _combatPool))
            {
                return;
            }

            ref var target = ref _targetPool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_transformPool.HasComponent(target.Id))
            {
                _targetPool.RemoveComponent(entity);
                _hitRequestPool.RemoveComponent(entity);
                _moveToPositionPool.RemoveComponent(entity);
                return;
            }
            
            var myPosition = _transformPool.GetComponent(entity).Value.position;
            var targetPosition = _transformPool.GetComponent(target.Id).Value.position;
            ref var minDistance = ref _combatPool.GetComponent(entity).MinDistance;

            if (Vector3.Distance(myPosition, targetPosition) <= minDistance)
            {
                //Attack target:
                _moveToPositionPool.RemoveComponent(entity);
                _hitRequestPool.SetComponent(entity, new HitRequest
                {
                    Target = target
                });
            }
            else
            {
                //Move to target:
                _hitRequestPool.RemoveComponent(entity);
                _moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    Destination = targetPosition,
                    StoppingDistance = Mathf.Max(0.1f, minDistance * 0.8f)
                });    
            }
        }
    }
}
