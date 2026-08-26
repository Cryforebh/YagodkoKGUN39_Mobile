using System;
using System.Collections.Generic;
using GameECS;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    [DefaultExecutionOrder(-2000)]
    [RequireComponent(typeof(Entity))]
    public abstract class EntityBehaviour : MonoBehaviour
    {
        private Entity _entity;
        private EcsWorld _world;
        private IEcsLoop _loop;
        private CompositeDisposable _loopSubscriptions;
        private bool _isConfigured;

        private readonly List<IEcsUpdate> _updateSystems = new();
        private readonly List<IEcsFixedUpdate> _fixedUpdateSystems = new();
        private readonly List<IEcsLateUpdate> _lateUpdateSystems = new();
        
        private readonly List<(Type, IEcsObserver)> _observers = new();

        protected abstract IEnumerable<IEcsSystem> ProvideSystems();
        protected abstract IEnumerable<(Type, IEcsObserver)> ProvideObservers();

        [Inject]
        private void Construct(EcsWorld ecsWorld, IEcsLoop ecsLoop)
        {
            _world = ecsWorld;
            _loop = ecsLoop;
            _entity = GetComponent<Entity>();

            var systems = this.ProvideSystems();
            this.RegisterSystems(systems);

            var observers = this.ProvideObservers();
            _observers.AddRange(observers);
            _isConfigured = true;

            if (isActiveAndEnabled)
            {
                SubscribeLifecycle();
            }
        }

        private void OnEnable()
        {
            if (_isConfigured)
            {
                SubscribeLifecycle();
            }
        }

        private void OnDisable()
        {
            if (!_isConfigured)
            {
                return;
            }

            _loopSubscriptions?.Dispose();
            _loopSubscriptions = null;
            UnsubscribeObservers();
        }

        private void SubscribeLifecycle()
        {
            if (_loopSubscriptions != null)
            {
                return;
            }

            _loopSubscriptions = new CompositeDisposable
            {
                _loop.Updated.Subscribe(_ => OnUpdate()),
                _loop.FixedUpdated.Subscribe(_ => OnFixedUpdate()),
                _loop.LateUpdated.Subscribe(_ => OnLateUpdate())
            };

            SubscribeObservers();
        }

        private void OnUpdate()
        {
            if (_entity.IsExists())
            {
                foreach (var state in _updateSystems)
                {
                    state.Update(_entity.Id);
                }
            }
        }

        private void OnFixedUpdate()
        {
            if (_entity.IsExists())
            {
                foreach (var state in _fixedUpdateSystems)
                {
                    state.FixedUpdate(_entity.Id);
                }
            }
        }

        private void OnLateUpdate()
        {
            if (_entity.IsExists())
            {
                foreach (var state in _lateUpdateSystems)
                {
                    state.LateUpdate(_entity.Id);
                }
            }
        }

        private void RegisterSystems(IEnumerable<object> systems)
        {
            foreach (var system in systems)
            {
                _world.Inject(system);

                if (system is IEcsUpdate update)
                {
                    _updateSystems.Add(update);
                }

                if (system is IEcsFixedUpdate fixedUpdate)
                {
                    _fixedUpdateSystems.Add(fixedUpdate);
                }

                if (system is IEcsLateUpdate lateUpdate)
                {
                    _lateUpdateSystems.Add(lateUpdate);
                }
            }
        }
        
        private void SubscribeObservers()
        {
            foreach (var (eventType, observer) in _observers)
            {
                _world.Subscribe(_entity.Id, eventType, observer);
            }
        }
        private void UnsubscribeObservers()
        {
            foreach (var (eventType, observer) in _observers)
            {
                _world.Unsubscribe(_entity.Id, eventType, observer);
            }
        }
    }
}
