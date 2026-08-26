using Game.GameEngine.Ecs;
using GameECS;
using NUnit.Framework;

namespace RolePlayingPrototype.Tests
{
    public sealed class CombatDamageTests
    {
        private const int ATTACK_COUNT = 100;
        private const int DAMAGE = 3;
        private const int INITIAL_HIT_POINTS = 1000;

        [Test]
        public void AttackAnimationEvent_AlwaysDealsConfiguredDamage()
        {
            var world = new EcsWorld();
            world.DeclareComponent<HitRequest>();
            world.DeclareComponent<CombatComponent>();
            world.DeclareComponent<HitPointsComponent>();
            world.DeclareObserver<AnimatorEvent, CharacterAnimatorObserver>();
            world.DeclareObserver<HitEvent, HitObserver_DealMeleeDamage>();
            world.DeclareObserver<TakeDamageEvent, TakeDamageObserver_DecrementHitPoints>();
            world.ResolveDependencies();

            var attackerId = world.CreateEntity();
            var targetId = world.CreateEntity();
            var target = world.GetEntityHandle(targetId);
            var combat = new CombatComponent { Damage = DAMAGE, DamageType = DamageType.MELEE };
            var request = new HitRequest { Target = target };
            var hitPoints = new HitPointsComponent { Current = INITIAL_HIT_POINTS, Max = INITIAL_HIT_POINTS };
            world.SetComponent(attackerId, ref combat);
            world.SetComponent(attackerId, ref request);
            world.SetComponent(targetId, ref hitPoints);

            for (var i = 0; i < ATTACK_COUNT; i++)
            {
                world.SendEvent(attackerId, new AnimatorEvent { Message = "attack" });
            }

            var expectedHitPoints = INITIAL_HIT_POINTS - ATTACK_COUNT * DAMAGE;
            Assert.That(world.GetComponent<HitPointsComponent>(targetId).Current, Is.EqualTo(expectedHitPoints));
        }
    }
}
