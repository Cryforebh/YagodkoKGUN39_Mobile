using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherDurationSystem : IEcsFixedUpdate
    {
        private EcsPool<GatherDuration> _durationPool;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_durationPool.HasComponent(entity))
            {
                return;
            }

            ref var duration = ref _durationPool.GetComponent(entity);
            duration.RemainingTime -= Time.fixedDeltaTime;

            if (duration.RemainingTime <= 0)
            {
                _durationPool.RemoveComponent(entity);
            }
        }
    }
}