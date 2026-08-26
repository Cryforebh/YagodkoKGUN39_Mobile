using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveStepSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<MoveStepData> _stepDataPool;
        private readonly EcsPool<MoveSpeedComponent> _speedPool;
        private readonly EcsPool<RigidbodyComponent> _rigidbodyPool;

        private readonly EcsEmitter<SmoothRotateEvent> _rotateEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _stepDataPool, _speedPool, _rigidbodyPool))
            {
                return;
            }

            ref var stepData = ref _stepDataPool.GetComponent(entity);
            if (stepData.Completed)
            {
                _stepDataPool.RemoveComponent(entity);
                return;
            }

            this.UpdatePosition(entity, stepData.Direction);
            this.UpdateRotation(entity, stepData.Direction);

            stepData.Completed = true;
        }

        private void UpdatePosition(int entity, Vector3 direction)
        {
            ref var rigidbody = ref _rigidbodyPool.GetComponent(entity).Value;
            ref var moveSpeed = ref _speedPool.GetComponent(entity).Value;

            var moveStep = direction * moveSpeed * Time.fixedDeltaTime;
            var newPosition = rigidbody.position + moveStep;
            rigidbody.MovePosition(newPosition);
        }

        private void UpdateRotation(int entity, Vector3 direction)
        {
            _rotateEmitter.SendEvent(entity, new SmoothRotateEvent
            {
                Direction = direction
            });
        }
    }
}
