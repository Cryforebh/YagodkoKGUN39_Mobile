using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitCountdownSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<HitCountdown> _countdownPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_countdownPool.HasComponent(entity))
            {
                return;
            }

            ref var countdown = ref _countdownPool.GetComponent(entity);
            countdown.RemainingTime -= Time.fixedDeltaTime;

            if (countdown.RemainingTime <= 0)
            {
                _countdownPool.RemoveComponent(entity);
            }
        }
    }
}