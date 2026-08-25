using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Team»",
        menuName = "Game/GameEngine/Ecs/New Installer «Team»"
    )]
    public sealed class TeamInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<TeamComponent>();
        }
    }
}