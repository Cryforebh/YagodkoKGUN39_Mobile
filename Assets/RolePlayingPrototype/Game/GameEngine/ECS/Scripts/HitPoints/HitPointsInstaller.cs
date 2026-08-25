using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Hit Points»",
        menuName = "Game/GameEngine/Ecs/New Installer «Hit Points»"
    )]
    public sealed class HitPointsInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<HitPointsComponent>();
        }
    }
}