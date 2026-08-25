using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Animation»",
        menuName = "Game/GameEngine/Ecs/New Installer «Animation»"
    )]
    public sealed class AnimationInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<AnimatorComponent>();
        }
    }
}