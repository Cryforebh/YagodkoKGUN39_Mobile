using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Commands»",
        menuName = "Game/GameEngine/Ecs/New Installer «Commands»"
    )]
    public sealed class CommandsInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<CommandRequest>();
        }
    }
}