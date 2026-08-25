using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitDurationSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<HitDuration> durationPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.durationPool.HasComponent(entity))
            {
                return;
            }

            var deltaTime = Time.fixedDeltaTime;

            ref var duration = ref this.durationPool.GetComponent(entity);
            duration.remainingTime -= deltaTime;

            if (duration.remainingTime <= 0.0f)
            {
                this.durationPool.RemoveComponent(entity);
            }
        }
    }
}