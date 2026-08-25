using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Combat»",
        menuName = "Game/GameEngine/Ecs/New Installer «Combat»"
    )]
    public sealed class CombatInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<CombatComponent>();

            world.DeclareComponent<HitCountdown>();
            world.DeclareComponent<HitRequest>();
            world.DeclareComponent<HitDuration>();
            world.DeclareComponent<AttackTarget>();

            world.DeclareSystem<HitRequestSystem>();
            world.DeclareSystem<HitCountdownSystem>();
            world.DeclareSystem<HitDurationSystem>();
            world.DeclareSystem<HitSystem_LookAtTarget>();
            world.DeclareSystem<AttackTargetSystem>();

            world.DeclareObserver<HitEvent, HitObserver_DealMeleeDamage>();
            world.DeclareObserver<HitEvent, HitObserver_ReloadAfterHit>();
        }
    }
}