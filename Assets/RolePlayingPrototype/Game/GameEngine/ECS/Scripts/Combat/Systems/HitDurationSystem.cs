using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class HitDurationSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<HitDuration> _durationPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_durationPool.HasComponent(entity))
            {
                return;
            }

            var deltaTime = Time.fixedDeltaTime;

            ref var duration = ref _durationPool.GetComponent(entity);
            duration.RemainingTime -= deltaTime;

            if (duration.RemainingTime <= 0.0f)
            {
                _durationPool.RemoveComponent(entity);
            }
        }
    }
}