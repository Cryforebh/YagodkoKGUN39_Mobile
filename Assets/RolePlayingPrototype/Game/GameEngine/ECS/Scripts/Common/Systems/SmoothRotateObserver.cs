using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class SmoothRotateObserver : IEcsObserver<SmoothRotateEvent>
    {
        private static readonly Vector3 UP = Vector3.up;
        private const float SMOOTH_TIME = 0.075f;
        private const float MAX_SPEED = 10000.0f;
        
        private EcsPool<SmoothRotationComponent> rotationPool;
        private EcsPool<RigidbodyComponent> rigidbodyPool;
        
        void IEcsObserver<SmoothRotateEvent>.Handle(int entity, SmoothRotateEvent smoothRotateEvent)
        {
            var direction = smoothRotateEvent.direction;
            ref var transform = ref this.rotationPool.GetComponent(entity);
            ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity).value;

            var currentRotation = rigidbody.rotation;
            var targetRotation = Quaternion.LookRotation(direction, UP);
            
            var currentAngle = currentRotation.eulerAngles.y;
            var targetAngle = targetRotation.eulerAngles.y;
            
            var newAngle = Mathf.SmoothDampAngle(
                currentAngle,
                targetAngle,
                ref transform.currentVelocity,
                SMOOTH_TIME,
                MAX_SPEED,
                Time.fixedDeltaTime
            );

            var newRotation = Quaternion.Euler(0.0f, newAngle, 0.0f);
            rigidbody.MoveRotation(newRotation);
        }
    }
}