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
        private Entity entity;
        private EcsWorld world;
        private IEcsLoop loop;
        private CompositeDisposable loopSubscriptions;
        private bool isConfigured;

        private readonly List<IEcsUpdate> updateSystems = new();
        private readonly List<IEcsFixedUpdate> fixedUpdateSystems = new();
        private readonly List<IEcsLateUpdate> lateUpdateSystems = new();
        
        private readonly List<(Type, IEcsObserver)> observers = new();

        protected abstract IEnumerable<IEcsSystem> ProvideSystems();
        protected abstract IEnumerable<(Type, IEcsObserver)> ProvideObservers();

        [Inject]
        private void Construct(EcsWorld ecsWorld, IEcsLoop ecsLoop)
        {
            world = ecsWorld;
            loop = ecsLoop;
            entity = GetComponent<Entity>();

            var systems = this.ProvideSystems();
            this.RegisterSystems(systems);

            var observers = this.ProvideObservers();
            this.observers.AddRange(observers);
            isConfigured = true;

            if (isActiveAndEnabled)
            {
                SubscribeLifecycle();
            }
        }

        private void OnEnable()
        {
            if (isConfigured)
            {
                SubscribeLifecycle();
            }
        }

        private void OnDisable()
        {
            if (!isConfigured)
            {
                return;
            }

            loopSubscriptions?.Dispose();
            loopSubscriptions = null;
            UnsubscribeObservers();
        }

        private void SubscribeLifecycle()
        {
            if (loopSubscriptions != null)
            {
                return;
            }

            loopSubscriptions = new CompositeDisposable
            {
                loop.Updated.Subscribe(_ => OnUpdate()),
                loop.FixedUpdated.Subscribe(_ => OnFixedUpdate()),
                loop.LateUpdated.Subscribe(_ => OnLateUpdate())
            };

            SubscribeObservers();
        }

        private void OnUpdate()
        {
            if (this.entity.IsExists())
            {
                foreach (var state in this.updateSystems)
                {
                    state.Update(this.entity.Id);
                }
            }
        }

        private void OnFixedUpdate()
        {
            if (this.entity.IsExists())
            {
                foreach (var state in this.fixedUpdateSystems)
                {
                    state.FixedUpdate(this.entity.Id);
                }
            }
        }

        private void OnLateUpdate()
        {
            if (this.entity.IsExists())
            {
                foreach (var state in this.lateUpdateSystems)
                {
                    state.LateUpdate(this.entity.Id);
                }
            }
        }

        private void RegisterSystems(IEnumerable<object> systems)
        {
            foreach (var system in systems)
            {
                world.Inject(system);

                if (system is IEcsUpdate update)
                {
                    this.updateSystems.Add(update);
                }

                if (system is IEcsFixedUpdate fixedUpdate)
                {
                    this.fixedUpdateSystems.Add(fixedUpdate);
                }

                if (system is IEcsLateUpdate lateUpdate)
                {
                    this.lateUpdateSystems.Add(lateUpdate);
                }
            }
        }
        
        private void SubscribeObservers()
        {
            foreach (var (eventType, observer) in this.observers)
            {
                world.Subscribe(this.entity.Id, eventType, observer);
            }
        }
        private void UnsubscribeObservers()
        {
            foreach (var (eventType, observer) in this.observers)
            {
                world.Unsubscribe(this.entity.Id, eventType, observer);
            }
        }
    }
}
