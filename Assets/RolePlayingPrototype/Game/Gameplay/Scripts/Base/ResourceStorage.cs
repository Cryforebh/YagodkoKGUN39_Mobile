using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using UniRx;

namespace SampleProject.Base
{
    public interface IResourceStorage
    {
        IReadOnlyReactiveProperty<int> Get(ResourceType type);
        bool CanSpend(ResourceType type, int amount);
        bool TrySpend(ResourceType type, int amount);
    }

    public sealed class ResourceStorage : IResourceStorage, IDisposable
    {
        private readonly Dictionary<ResourceType, ReactiveProperty<int>> _amounts = new();
        private readonly CompositeDisposable _subscriptions = new();

        public ResourceStorage(IEcsEventStream eventStream)
        {
            _amounts.Add(ResourceType.Minerals, new ReactiveProperty<int>());
            _amounts.Add(ResourceType.Wood, new ReactiveProperty<int>());
            _amounts.Add(ResourceType.Crystals, new ReactiveProperty<int>());
            eventStream.Observe<ResourceDeliveredEvent>().Subscribe(message => Add(message.Value)).AddTo(_subscriptions);
        }

        public IReadOnlyReactiveProperty<int> Get(ResourceType type)
        {
            return _amounts[type];
        }

        public bool CanSpend(ResourceType type, int amount)
        {
            return amount >= 0 && _amounts.TryGetValue(type, out var stored) && stored.Value >= amount;
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (!CanSpend(type, amount))
            {
                return false;
            }

            _amounts[type].Value -= amount;
            return true;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            foreach (var amount in _amounts.Values)
            {
                amount.Dispose();
            }

            _amounts.Clear();
        }

        private void Add(ResourceDeliveredEvent delivered)
        {
            _amounts[delivered.ResourceType].Value += delivered.ResourceAmount;
        }
    }
}
