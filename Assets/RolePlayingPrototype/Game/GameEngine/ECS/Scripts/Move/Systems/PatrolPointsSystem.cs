using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class PatrolPointsSystem : IEcsFixedUpdate
    {
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<PatrolData> _patrolPool;
        private EcsPool<MoveToPositionData> _movePool;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _patrolPool, _transformPool))
            {
                return;
            }
            
            ref var transform = ref _transformPool.GetComponent(entity).Value;
            ref var patrolData = ref _patrolPool.GetComponent(entity);
            
            var targetPoint = patrolData.GetCurrentPoint();
            
            if (Vector3.Distance(transform.position, targetPoint) <= patrolData.StoppingDistance)
            {
                patrolData.MoveNext();
                return;
            }

            _movePool.SetComponent(entity, new MoveToPositionData
            {
                Destination = targetPoint,
                StoppingDistance = patrolData.StoppingDistance
            });
        }
    }
}
