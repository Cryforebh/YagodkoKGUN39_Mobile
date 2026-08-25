using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Take Damage»",
        menuName = "Game/GameEngine/Ecs/New Installer «Take Damage»"
    )]
    public sealed class TakeDamageInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareObserver<TakeDamageEvent, TakeDamageObserver_DecrementHitPoints>();
            world.DeclareObserver<TakeDamageEvent, TakeDamageObserver_ChangeColor>();
        }
    }
}