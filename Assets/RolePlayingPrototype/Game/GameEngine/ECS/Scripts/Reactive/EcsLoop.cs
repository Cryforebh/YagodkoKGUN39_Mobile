using System;
using GameECS;
using UniRx;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public interface IEcsLoop
    {
        IObservable<Unit> Updated { get; }
        IObservable<Unit> FixedUpdated { get; }
        IObservable<Unit> LateUpdated { get; }
    }

    public sealed class EcsLoop : IEcsLoop, IInitializable, ITickable, IFixedTickable, ILateTickable, IDisposable
    {
        private readonly EcsWorld world;
        private readonly Subject<Unit> updated = new();
        private readonly Subject<Unit> fixedUpdated = new();
        private readonly Subject<Unit> lateUpdated = new();

        public IObservable<Unit> Updated => updated;
        public IObservable<Unit> FixedUpdated => fixedUpdated;
        public IObservable<Unit> LateUpdated => lateUpdated;

        public EcsLoop(EcsWorld world)
        {
            this.world = world;
        }

        public void Initialize()
        {
            world.ResolveDependencies();
        }

        public void Tick()
        {
            world.Update();
            updated.OnNext(Unit.Default);
        }

        public void FixedTick()
        {
            world.FixedUpdate();
            fixedUpdated.OnNext(Unit.Default);
        }

        public void LateTick()
        {
            world.LateUpdate();
            lateUpdated.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            updated.Dispose();
            fixedUpdated.Dispose();
            lateUpdated.Dispose();
        }
    }
}
