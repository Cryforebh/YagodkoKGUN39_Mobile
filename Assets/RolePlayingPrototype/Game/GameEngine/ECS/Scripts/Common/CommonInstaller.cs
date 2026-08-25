using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Common»",
        menuName = "Game/GameEngine/Ecs/New Installer «Common»"
    )]
    public sealed class CommonInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<TransformComponent>();
            world.DeclareComponent<GameObjectComponent>();
            world.DeclareComponent<RendererComponent>();
            world.DeclareComponent<RigidbodyComponent>();
            world.DeclareComponent<SmoothRotationComponent>();

            world.DeclareObserver<SmoothRotateEvent, SmoothRotateObserver>();
        }
    }
}