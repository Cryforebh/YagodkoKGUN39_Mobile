using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class DestroyObserver_DisableGameObject : IEcsObserver<DestroyEvent>
    {
        private readonly EcsEmitter<DestroyEvent> _destroyEmitter;
        private readonly EcsPool<GameObjectComponent> _gameObjectPool;

        void IEcsObserver<DestroyEvent>.Handle(int entity, DestroyEvent destroyEvent)
        {
            ref var goComponent = ref _gameObjectPool.GetComponent(entity);
            goComponent.Value.SetActive(false);
        }
    }
}