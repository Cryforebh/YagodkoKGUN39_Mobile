using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackTargetSystem : IEcsFixedUpdate
    {
        private EcsPool<AttackTarget> targetPool;
        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;

        private EcsPool<CombatComponent> combatPool;
        private EcsPool<TransformComponent> transformPool;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.targetPool.HasComponent(entity))
            {
                return;
            }

            ref var targetId = ref this.targetPool.GetComponent(entity).targetId;
            
            var myPosition = this.transformPool.GetComponent(entity).value.position;
            var targetPosition = this.transformPool.GetComponent(targetId).value.position;
            ref var minDistance = ref this.combatPool.GetComponent(entity).minDistance;
            
            if (Vector3.Distance(myPosition, targetPosition) <= minDistance)
            {
                //Attack target:
                this.moveToPositionPool.RemoveComponent(entity);
                this.hitRequestPool.SetComponent(entity, new HitRequest
                {
                    targetId = targetId
                });
            }
            else
            {
                //Move to target:
                this.hitRequestPool.RemoveComponent(entity);
                this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    destination = targetPosition,
                    stoppingDistance = minDistance
                });    
            }
        }
    }
}