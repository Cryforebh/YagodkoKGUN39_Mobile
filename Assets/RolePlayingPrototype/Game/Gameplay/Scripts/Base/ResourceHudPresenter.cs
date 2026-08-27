using System;
using Game.GameEngine.Ecs;
using UniRx;
using Zenject;

namespace SampleProject.Base
{
    public sealed class ResourceHudPresenter : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new();
        private readonly IResourceStorage _storage;
        private readonly GameplayHudView _view;

        public ResourceHudPresenter(IResourceStorage storage, GameplayHudView view)
        {
            _storage = storage;
            _view = view;
        }

        public void Initialize()
        {
            BindLabel(ResourceType.Minerals, _view.MineralsLabel);
            BindLabel(ResourceType.Wood, _view.WoodLabel);
            BindLabel(ResourceType.Crystals, _view.CrystalsLabel);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        private void BindLabel(ResourceType type, TMPro.TextMeshProUGUI label)
        {
            _storage.Get(type).Subscribe(value => label.text = $"{type}: {value}").AddTo(_subscriptions);
        }
    }
}
