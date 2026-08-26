using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class DeathAnimationSystem : IEcsUpdate
    {
        private EcsPool<DeathAnimationComponent> _deathPool;
        private EcsPool<DeathSettingsComponent> _settingsPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsEmitter<DestroyEvent> _destroyEmitter;

        void IEcsUpdate.Update(int entity)
        {
            if (!EcsFilter.Matches(entity, _deathPool, _settingsPool, _transformPool))
            {
                return;
            }

            ref var death = ref _deathPool.GetComponent(entity);
            var duration = Mathf.Max(0.1f, _settingsPool.GetComponent(entity).Duration);
            death.ElapsedTime += Time.deltaTime;
            var progress = Mathf.Clamp01(death.ElapsedTime / duration);
            _transformPool.GetComponent(entity).Value.rotation = Quaternion.Slerp(death.StartRotation, death.TargetRotation, progress);

            if (progress >= 1f)
            {
                _destroyEmitter.SendEvent(entity, new DestroyEvent());
            }
        }
    }
}
