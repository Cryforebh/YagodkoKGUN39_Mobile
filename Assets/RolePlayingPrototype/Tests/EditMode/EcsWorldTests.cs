using GameECS;
using NUnit.Framework;

namespace RolePlayingPrototype.Tests
{
    public sealed class EcsWorldTests
    {
        private struct TestComponent
        {
            public int Value;
        }

        private struct TestEvent
        {
        }

        private sealed class DestroyEntitySystem : IEcsUpdate
        {
            private EcsWorld _world;

            public DestroyEntitySystem()
            {
            }

            public void Update(int entity)
            {
                _world.DestroyEntity(entity);
            }
        }

        private sealed class CountingSystem : IEcsUpdate
        {
            public static int Calls { get; set; }

            public CountingSystem()
            {
            }

            public void Update(int entity)
            {
                Calls++;
            }
        }

        private sealed class ReplaceEntitySystem : IEcsUpdate
        {
            private EcsWorld _world;

            public ReplaceEntitySystem()
            {
            }

            public void Update(int entity)
            {
                _world.DestroyEntity(entity);
                _world.CreateEntity();
            }
        }

        [Test]
        public void DestroyEntity_RemovesComponents_AndReusesId()
        {
            var world = new EcsWorld();
            world.DeclareComponent<TestComponent>();
            var firstId = world.CreateEntity();
            var component = new TestComponent { Value = 42 };
            world.SetComponent(firstId, ref component);

            world.DestroyEntity(firstId);
            var reusedId = world.CreateEntity();

            Assert.That(reusedId, Is.EqualTo(firstId));
            Assert.That(world.IsEntityExists(reusedId), Is.True);
            Assert.That(world.HasComponent<TestComponent>(reusedId), Is.False);
        }

        [Test]
        public void DestroyEntity_RemovesEntityEventSubscriptions()
        {
            var world = new EcsWorld();
            var entity = world.CreateEntity();
            var received = 0;
            world.Subscribe<TestEvent>(entity, _ => received++);

            world.DestroyEntity(entity);
            var reusedEntity = world.CreateEntity();
            world.SendEvent(reusedEntity, new TestEvent());

            Assert.That(reusedEntity, Is.EqualTo(entity));
            Assert.That(received, Is.Zero);
        }

        [Test]
        public void DestroyEntity_IsSafeWhenCalledMoreThanOnce()
        {
            var world = new EcsWorld();
            var entity = world.CreateEntity();

            world.DestroyEntity(entity);

            Assert.DoesNotThrow(() => world.DestroyEntity(entity));
        }

        [Test]
        public void ReusedId_InvalidatesPreviousHandle()
        {
            var world = new EcsWorld();
            var firstId = world.CreateEntity();
            var firstHandle = world.GetEntityHandle(firstId);
            world.DestroyEntity(firstHandle);
            var secondId = world.CreateEntity();
            var secondHandle = world.GetEntityHandle(secondId);

            Assert.That(secondId, Is.EqualTo(firstId));
            Assert.That(secondHandle.Generation, Is.Not.EqualTo(firstHandle.Generation));
            Assert.That(world.IsEntityExists(firstHandle), Is.False);
            Assert.That(world.IsEntityExists(secondHandle), Is.True);

            world.DestroyEntity(firstHandle);

            Assert.That(world.IsEntityExists(secondHandle), Is.True);
        }

        [Test]
        public void Update_DoesNotPassDestroyedEntityToFollowingSystems()
        {
            CountingSystem.Calls = 0;
            var world = new EcsWorld();
            world.DeclareSystem<DestroyEntitySystem>();
            world.DeclareSystem<CountingSystem>();
            world.ResolveDependencies();
            world.CreateEntity();

            world.Update();

            Assert.That(CountingSystem.Calls, Is.Zero);
        }

        [Test]
        public void Update_DoesNotProcessReplacementEntityUntilNextFrame()
        {
            CountingSystem.Calls = 0;
            var world = new EcsWorld();
            world.DeclareSystem<ReplaceEntitySystem>();
            world.DeclareSystem<CountingSystem>();
            world.ResolveDependencies();
            world.CreateEntity();

            world.Update();

            Assert.That(CountingSystem.Calls, Is.Zero);
        }

        [Test]
        public void Filter_MatchesOnlyCompleteComponentSet()
        {
            var world = new EcsWorld();
            world.DeclareComponent<TestComponent>();
            world.DeclareComponent<TestEvent>();
            world.CreateEntity();
            var firstPool = world.GetPool<TestComponent>();
            var secondPool = world.GetPool<TestEvent>();
            firstPool.SetComponent(0, new TestComponent());

            Assert.That(EcsFilter.Matches(0, firstPool, secondPool), Is.False);

            secondPool.SetComponent(0, new TestEvent());

            Assert.That(EcsFilter.Matches(0, firstPool, secondPool), Is.True);
        }
    }
}
