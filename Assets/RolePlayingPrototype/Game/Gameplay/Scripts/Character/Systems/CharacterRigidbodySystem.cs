using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace SampleProject
{
    public sealed class CharacterRigidbodySystem : IEcsFixedUpdate
    {
        private const RigidbodyConstraints FREEZE = RigidbodyConstraints.FreezePositionX |
                                                    RigidbodyConstraints.FreezePositionY |
                                                    RigidbodyConstraints.FreezePositionZ |
                                                    RigidbodyConstraints.FreezeRotationX |
                                                    RigidbodyConstraints.FreezeRotationZ;

        private const RigidbodyConstraints UNFREEZE = RigidbodyConstraints.FreezePositionY |
                                                      RigidbodyConstraints.FreezeRotationX |
                                                      RigidbodyConstraints.FreezeRotationZ;

        private EcsPool<RigidbodyComponent> _rigidbodyPool;
        private EcsPool<HitDuration> _attackPool;
        private EcsPool<GatherDuration> _gatherPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_rigidbodyPool.HasComponent(entity))
            {
                return;
            }

            ref var rigidbody = ref _rigidbodyPool.GetComponent(entity).Value;
            var freeze = IsFreeze(entity);
            var constraints = freeze ? FREEZE : UNFREEZE;
            if (rigidbody.constraints != constraints)
            {
                rigidbody.constraints = constraints;
            }
        }

        private bool IsFreeze(int entity)
        {
            if (_attackPool.HasComponent(entity))
            {
                return true;
            }

            if (_gatherPool.HasComponent(entity))
            {
                return true;
            }

            return false;
        }
    }
}
