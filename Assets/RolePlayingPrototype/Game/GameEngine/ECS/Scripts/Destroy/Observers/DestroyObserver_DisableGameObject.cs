using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class DestroyObserver_DisableGameObject : IEcsObserver<DestroyEvent>
    {
        private readonly EcsEmitter<DestroyEvent> destroyEmitter;
        private readonly EcsPool<GameObjectComponent> gameObjectPool;

        void IEcsObserver<DestroyEvent>.Handle(int entity, DestroyEvent destroyEvent)
        {
            ref var goComponent = ref this.gameObjectPool.GetComponent(entity);
            goComponent.value.SetActive(false);
        }
    }
}