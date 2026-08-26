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
        private readonly EcsWorld _world;
        private readonly Subject<Unit> _updated = new();
        private readonly Subject<Unit> _fixedUpdated = new();
        private readonly Subject<Unit> _lateUpdated = new();

        public IObservable<Unit> Updated => _updated;
        public IObservable<Unit> FixedUpdated => _fixedUpdated;
        public IObservable<Unit> LateUpdated => _lateUpdated;

        public EcsLoop(EcsWorld world)
        {
            _world = world;
        }

        public void Initialize()
        {
            _world.ResolveDependencies();
        }

        public void Tick()
        {
            _world.Update();
            _updated.OnNext(Unit.Default);
        }

        public void FixedTick()
        {
            _world.FixedUpdate();
            _fixedUpdated.OnNext(Unit.Default);
        }

        public void LateTick()
        {
            _world.LateUpdate();
            _lateUpdated.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _updated.Dispose();
            _fixedUpdated.Dispose();
            _lateUpdated.Dispose();
        }
    }
}
