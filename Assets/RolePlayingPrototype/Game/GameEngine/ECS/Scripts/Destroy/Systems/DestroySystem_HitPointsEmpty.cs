using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class DestroySystem_HitPointsEmpty : IEcsFixedUpdate
    {
        private readonly EcsPool<HitPointsComponent> _hitPointsPool;
        private readonly EcsPool<DeathAnimationComponent> _deathPool;
        private readonly EcsPool<DeathSettingsComponent> _deathSettingsPool;
        private readonly EcsPool<TransformComponent> _transformPool;
        private readonly EcsPool<CommandRequest> _commandPool;
        private readonly EcsPool<AttackTarget> _attackPool;
        private readonly EcsPool<HitRequest> _hitRequestPool;
        private readonly EcsPool<MoveToPositionData> _movePool;
        private readonly EcsPool<MoveStepData> _moveStepPool;
        private readonly EcsPool<RigidbodyComponent> _rigidbodyPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _hitPointsPool, _transformPool, _deathSettingsPool))
            {
                return;
            }

            ref var hitPoints = ref _hitPointsPool.GetComponent(entity);
            if (hitPoints.Current <= 0 && !_deathPool.HasComponent(entity))
            {
                var rotation = _transformPool.GetComponent(entity).Value.rotation;
                _deathPool.SetComponent(entity, new DeathAnimationComponent
                {
                    StartRotation = rotation,
                    TargetRotation = rotation * Quaternion.Euler(90f, 0f, 0f)
                });
                _commandPool.RemoveComponent(entity);
                _attackPool.RemoveComponent(entity);
                _hitRequestPool.RemoveComponent(entity);
                _movePool.RemoveComponent(entity);
                _moveStepPool.RemoveComponent(entity);

                if (_rigidbodyPool.HasComponent(entity))
                {
                    _rigidbodyPool.GetComponent(entity).Value.isKinematic = true;
                }
            }
        }
    }
}
