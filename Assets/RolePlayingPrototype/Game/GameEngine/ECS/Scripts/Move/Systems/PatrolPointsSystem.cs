using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class PatrolPointsSystem : IEcsFixedUpdate
    {
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<PatrolData> patrolPool;
        private EcsPool<MoveToPositionData> movePool;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.patrolPool.HasComponent(entity))
            {
                return;
            }
            
            ref var transform = ref this.transformPool.GetComponent(entity).value;
            ref var patrolData = ref this.patrolPool.GetComponent(entity);
            
            var targetPoint = patrolData.GetCurrentPoint();
            
            if (Vector3.Distance(transform.position, targetPoint) <= patrolData.stoppingDistance)
            {
                patrolData.MoveNext();
                return;
            }

            this.movePool.SetComponent(entity, new MoveToPositionData
            {
                destination = targetPoint
            });
        }
    }
}