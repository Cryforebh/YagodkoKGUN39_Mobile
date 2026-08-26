using System;
using Game.GameEngine.Ecs;
using GameECS;
using UniRx;

namespace SampleProject
{
    public interface IUnitSelectionService
    {
        IReadOnlyReactiveCollection<EntityHandle> Selected { get; }
        bool Select(EntityHandle entity, bool additive);
        void Deselect(EntityHandle entity);
        void Clear();
    }

    public sealed class UnitSelectionService : IUnitSelectionService, IDisposable
    {
        private readonly ReactiveCollection<EntityHandle> _selected = new();
        private readonly CompositeDisposable _subscriptions = new();
        private readonly EcsWorld _world;

        public IReadOnlyReactiveCollection<EntityHandle> Selected => _selected;

        public UnitSelectionService(EcsWorld world, IEcsEventStream eventStream)
        {
            _world = world;
            eventStream.Observe<EntityDestroyedEvent>().Subscribe(message => RemoveDestroyed(message.Entity)).AddTo(_subscriptions);
        }

        public bool Select(EntityHandle entity, bool additive)
        {
            if (!CanSelect(entity))
            {
                return false;
            }

            if (!additive)
            {
                Clear();
            }

            if (_selected.Contains(entity))
            {
                return true;
            }

            var marker = new SelectedComponent();
            _world.SetComponent(entity.Id, ref marker);
            _selected.Add(entity);
            return true;
        }

        public void Deselect(EntityHandle entity)
        {
            if (!_selected.Remove(entity))
            {
                return;
            }

            if (_world.IsEntityExists(entity))
            {
                _world.RemoveComponent<SelectedComponent>(entity.Id);
            }
        }

        public void Clear()
        {
            while (_selected.Count > 0)
            {
                Deselect(_selected[_selected.Count - 1]);
            }
        }

        public void Dispose()
        {
            Clear();
            _subscriptions.Dispose();
            _selected.Dispose();
        }

        private bool CanSelect(EntityHandle entity)
        {
            if (!_world.IsEntityExists(entity) || !_world.HasComponent<TeamComponent>(entity.Id))
            {
                return false;
            }

            return _world.GetComponent<TeamComponent>(entity.Id).Value == TeamId.Player;
        }

        private void RemoveDestroyed(int entityId)
        {
            for (var i = _selected.Count - 1; i >= 0; i--)
            {
                if (_selected[i].Id == entityId)
                {
                    _selected.RemoveAt(i);
                }
            }
        }
    }
}
