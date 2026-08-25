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

        private EcsPool<RigidbodyComponent> rigidbodyPool;
        private EcsPool<HitDuration> attackPool;
        private EcsPool<GatherDuration> gatherPool;

        private bool isFreeze;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity).value;

            var freeze = this.IsFreeze(entity);
            
            if (freeze && !this.isFreeze)
            {
                rigidbody.constraints = FREEZE;
                this.isFreeze = true;
                return;
            }

            if (!freeze && this.isFreeze)
            {
                rigidbody.constraints = UNFREEZE;
                this.isFreeze = false;
            }
        }

        private bool IsFreeze(int entity)
        {
            if (this.attackPool.HasComponent(entity))
            {
                return true;
            }

            if (this.gatherPool.HasComponent(entity))
            {
                return true;
            }

            return false;
        }
    }
}