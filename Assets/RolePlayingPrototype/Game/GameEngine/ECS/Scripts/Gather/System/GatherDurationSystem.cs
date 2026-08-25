using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherDurationSystem : IEcsFixedUpdate
    {
        private EcsPool<GatherDuration> durationPool;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.durationPool.HasComponent(entity))
            {
                return;
            }

            ref var duration = ref this.durationPool.GetComponent(entity);
            duration.remainingTime -= Time.fixedDeltaTime;

            if (duration.remainingTime <= 0)
            {
                this.durationPool.RemoveComponent(entity);
            }
        }
    }
}