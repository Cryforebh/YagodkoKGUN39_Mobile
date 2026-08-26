using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveToPositionSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<TransformComponent> _transformPool;
        private readonly EcsPool<MoveToPositionData> _moveToPositionPool;
        private readonly EcsPool<MoveStepData> _moveStepPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _moveToPositionPool, _transformPool))
            {
                return;
            }

            ref var moveData = ref _moveToPositionPool.GetComponent(entity);
            ref var transform = ref _transformPool.GetComponent(entity);

            var currentPosiiton = transform.Value.position;
            var targetPosition = moveData.Destination;
            var distanceVector = targetPosition - currentPosiiton;

            moveData.IsReached = distanceVector.sqrMagnitude <= moveData.StoppingDistance * moveData.StoppingDistance;
            if (moveData.IsReached)
            {
                return;
            }

            _moveStepPool.SetComponent(entity, new MoveStepData
            {
                Direction = distanceVector.normalized
            });
        }
    }
}
