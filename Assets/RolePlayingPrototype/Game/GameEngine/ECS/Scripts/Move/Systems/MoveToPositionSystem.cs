using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveToPositionSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<TransformComponent> transformPool;
        private readonly EcsPool<MoveToPositionData> moveToPositionPool;
        private readonly EcsPool<MoveStepData> moveStepPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.moveToPositionPool.HasComponent(entity))
            {
                return;
            }

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            ref var transform = ref this.transformPool.GetComponent(entity);

            var currentPosiiton = transform.value.position;
            var targetPosition = moveData.destination;
            var distanceVector = targetPosition - currentPosiiton;

            moveData.isReached = distanceVector.sqrMagnitude <= moveData.stoppingDistance;
            if (moveData.isReached)
            {
                return;
            }

            this.moveStepPool.SetComponent(entity, new MoveStepData
            {
                direction = distanceVector.normalized
            });
        }
    }
}