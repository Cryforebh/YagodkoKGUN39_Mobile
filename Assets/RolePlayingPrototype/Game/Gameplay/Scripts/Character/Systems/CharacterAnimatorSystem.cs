using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace SampleProject
{
    public sealed class CharacterAnimatorSystem : IEcsUpdate
    {
        private const float MinimumMovementDistance = 0.005f;

        private EcsPool<AnimatorComponent> _animatorPool;

        private EcsPool<MoveStepData> _moveStep;
        private EcsPool<HitDuration> _attackPool;
        private EcsPool<GatherDuration> _gatherPool;
        private EcsPool<DeathAnimationComponent> _deathPool;
        private EcsPool<TransformComponent> _transformPool;

        private Vector3 _lastPosition;
        private float _movingUntil;
        private bool _positionInitialized;

        void IEcsUpdate.Update(int entity)
        {
            UpdateMovementState(entity);
            ref var animator = ref _animatorPool.GetComponent(entity).Value;
            var animatorState = ResolveState(entity);
            animator.ChangeState(animatorState);
        }

        private int ResolveState(int entity)
        {
            if (_deathPool.HasComponent(entity))
            {
                return AnimatorStateId.IDLE;
            }

            if (_attackPool.HasComponent(entity))
            {
                return AnimatorStateId.ATTACK;
            }

            if (_gatherPool.HasComponent(entity))
            {
                return AnimatorStateId.GATHERING;
            }

            if (_moveStep.HasComponent(entity) && Time.time <= _movingUntil)
            {
                return AnimatorStateId.MOVE;
            }

            return AnimatorStateId.IDLE;
        }

        private void UpdateMovementState(int entity)
        {
            if (!_transformPool.HasComponent(entity))
            {
                return;
            }

            var position = _transformPool.GetComponent(entity).Value.position;
            if (_positionInitialized && Vector3.ProjectOnPlane(position - _lastPosition, Vector3.up).sqrMagnitude > MinimumMovementDistance * MinimumMovementDistance)
            {
                _movingUntil = Time.time + 0.15f;
            }

            _lastPosition = position;
            _positionInitialized = true;
        }
    }
}
