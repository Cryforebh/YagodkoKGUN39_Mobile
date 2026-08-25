using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class DestroySystem_HitPointsEmpty : IEcsFixedUpdate
    {
        private readonly EcsPool<HitPointsComponent> hitPointsPool;
        private readonly EcsEmitter<DestroyEvent> destroyEmitter;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.hitPointsPool.HasComponent(entity))
            {
                return;
            }

            ref var hitPoints = ref this.hitPointsPool.GetComponent(entity);
            if (hitPoints.current <= 0)
            {
                this.destroyEmitter.SendEvent(entity, new DestroyEvent());
            }
        }
    }
}
