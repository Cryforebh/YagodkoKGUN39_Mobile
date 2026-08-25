using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveStepSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<MoveStepData> stepDataPool;
        private readonly EcsPool<MoveSpeedComponent> speedPool;
        private readonly EcsPool<RigidbodyComponent> rigidbodyPool;

        private readonly EcsEmitter<SmoothRotateEvent> rotateEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.stepDataPool.HasComponent(entity))
            {
                return;
            }

            ref var stepData = ref this.stepDataPool.GetComponent(entity);
            if (stepData.completed)
            {
                this.stepDataPool.RemoveComponent(entity);
                return;
            }

            this.UpdatePosition(entity, stepData.direction);
            this.UpdateRotation(entity, stepData.direction);

            stepData.completed = true;
        }

        private void UpdatePosition(int entity, Vector3 direction)
        {
            ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity).value;
            ref var moveSpeed = ref this.speedPool.GetComponent(entity).value;

            var moveStep = direction * moveSpeed * Time.fixedDeltaTime;
            var newPosition = rigidbody.position + moveStep;
            rigidbody.MovePosition(newPosition);
        }

        private void UpdateRotation(int entity, Vector3 direction)
        {
            this.rotateEmitter.SendEvent(entity, new SmoothRotateEvent
            {
                direction = direction
            });
        }
    }
}