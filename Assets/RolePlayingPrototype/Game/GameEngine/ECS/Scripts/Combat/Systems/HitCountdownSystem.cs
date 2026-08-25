using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitCountdownSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<HitCountdown> countdownPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.countdownPool.HasComponent(entity))
            {
                return;
            }

            ref var countdown = ref this.countdownPool.GetComponent(entity);
            countdown.remainingTime -= Time.fixedDeltaTime;

            if (countdown.remainingTime <= 0)
            {
                this.countdownPool.RemoveComponent(entity);
            }
        }
    }
}