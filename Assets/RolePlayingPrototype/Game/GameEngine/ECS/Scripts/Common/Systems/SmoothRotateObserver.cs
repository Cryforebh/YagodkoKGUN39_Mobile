using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class SmoothRotateObserver : IEcsObserver<SmoothRotateEvent>
    {
        private static readonly Vector3 _uP = Vector3.up;
        private const float SMOOTH_TIME = 0.075f;
        private const float MAX_SPEED = 10000.0f;
        
        private EcsPool<SmoothRotationComponent> _rotationPool;
        private EcsPool<RigidbodyComponent> _rigidbodyPool;
        
        void IEcsObserver<SmoothRotateEvent>.Handle(int entity, SmoothRotateEvent smoothRotateEvent)
        {
            var direction = smoothRotateEvent.Direction;
            ref var transform = ref _rotationPool.GetComponent(entity);
            ref var rigidbody = ref _rigidbodyPool.GetComponent(entity).Value;

            var currentRotation = rigidbody.rotation;
            var targetRotation = Quaternion.LookRotation(direction, _uP);
            
            var currentAngle = currentRotation.eulerAngles.y;
            var targetAngle = targetRotation.eulerAngles.y;
            
            var newAngle = Mathf.SmoothDampAngle(
                currentAngle,
                targetAngle,
                ref transform.CurrentVelocity,
                SMOOTH_TIME,
                MAX_SPEED,
                Time.fixedDeltaTime
            );

            var newRotation = Quaternion.Euler(0.0f, newAngle, 0.0f);
            rigidbody.MoveRotation(newRotation);
        }
    }
}