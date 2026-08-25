using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameECS
{
    public sealed class EcsWorld
    {
        private readonly Dictionary<Type, IEcsPool> componentPools = new();
        private readonly Dictionary<Type, IEcsEmitter> eventEmitters = new();

        private readonly List<IEcsSystem> allSystems = new();
        private readonly List<IEcsUpdate> updateSystems = new();
        private readonly List<IEcsFixedUpdate> fixedUpdateSystems = new();
        private readonly List<IEcsLateUpdate> lateUpdateSystems = new();

        private readonly List<bool> entities = new();
        private readonly List<int> cache = new();

        private readonly List<object> externalServices = new();
        private readonly IEcsEventSink eventSink;
        private Action<object> externalInjector;

        public EcsWorld(IEcsEventSink eventSink = null)
        {
            this.eventSink = eventSink;
        }

        public void SetExternalInjector(Action<object> injector)
        {
            this.externalInjector = injector;
        }

        #region Entities

        public int CreateEntity()
        {
            var id = 0;
            var count = this.entities.Count;

            for (; id < count; id++)
            {
                if (!this.entities[id])
                {
                    this.entities[id] = true;
                    this.eventSink?.Publish(id, new EntityCreatedEvent());
                    return id;
                }
            }

            id = count;
            this.entities.Add(true);

            foreach (var pool in this.componentPools.Values)
            {
                pool.AllocComponent();
            }

            this.eventSink?.Publish(id, new EntityCreatedEvent());
            return id;
        }

        public bool IsEntityExists(int entity)
        {
            if (entity < 0 || entity >= this.entities.Count)
            {
                return false;
            }

            return this.entities[entity];
        }

        public void DestroyEntity(int entity)
        {
            this.eventSink?.Publish(entity, new EntityDestroyedEvent());
            this.entities[entity] = false;

            foreach (var componentPool in this.componentPools.Values)
            {
                componentPool.RemoveComponent(entity);
            }
        }

        #endregion

        #region Components

        public ref T GetComponent<T>(int entity) where T : struct
        {
            if (!this.componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            var tComponentPool = (EcsPool<T>) componentPool;
            return ref tComponentPool.GetComponent(entity);
        }

        public void SetComponent<T>(int entity, ref T component) where T : struct
        {
            if (!this.componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            var tComponentPool = (EcsPool<T>) componentPool;
            tComponentPool.SetComponent(entity, ref component);
        }

        public void RemoveComponent<T>(int entity) where T : struct
        {
            if (!this.componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            componentPool.RemoveComponent(entity);
        }

        public bool HasComponent<T>(int entity) where T : struct
        {
            if (!this.componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            return componentPool.HasComponent(entity);
        }

        internal object GetRawComponent(int entity, Type type)
        {
            var componentPool = this.componentPools[type];
            return componentPool.GetRawComponent(entity);
        }

        internal void SetRawComponent(int entity, object data)
        {
            var componentPool = this.componentPools[data.GetType()];
            componentPool.SetRawComponent(entity, data);
        }

        public List<object> GetRawComponents(int entityId)
        {
            var result = new List<object>();
            foreach (var pool in this.componentPools.Values)
            {
                if (pool.HasComponent(entityId))
                {
                    var component = pool.GetRawComponent(entityId);
                    result.Add(component);
                }
            }

            return result;
        }

        public EcsPool<T> GetPool<T>() where T : struct
        {
            if (!this.componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            return (EcsPool<T>) componentPool;
        }

        #endregion

        #region Events

        public void SendEvent<T>(int entity, T @event) where T : struct
        {
            if (!this.eventEmitters.TryGetValue(typeof(T), out var emitter))
            {
                emitter = new EcsEmitter<T>(this.eventSink);
                this.eventEmitters.Add(typeof(T), emitter);
            }

            var tEmitter = (EcsEmitter<T>) emitter;
            tEmitter.SendEvent(entity, @event);
        }

        public EcsEmitter<T> GetEmitter<T>() where T : struct
        {
            if (!this.eventEmitters.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            return (EcsEmitter<T>) componentPool;
        }

        #endregion

        #region Update

        public void Update()
        {
            this.cache.Clear();
            for (int id = 0, count = this.entities.Count; id < count; id++)
            {
                if (this.entities[id])
                {
                    this.cache.Add(id);
                }
            }

            var entityCount = this.cache.Count;

            for (int i = 0, count = this.updateSystems.Count; i < count; i++)
            {
                var system = this.updateSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var id = this.cache[e];
                    system.Update(id);
                }
            }
        }

        public void FixedUpdate()
        {
            this.cache.Clear();
            for (int id = 0, count = this.entities.Count; id < count; id++)
            {
                if (this.entities[id])
                {
                    this.cache.Add(id);
                }
            }

            var entityCount = this.cache.Count;

            for (int i = 0, count = this.fixedUpdateSystems.Count; i < count; i++)
            {
                var system = this.fixedUpdateSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var id = this.cache[e];
                    system.FixedUpdate(id);
                }
            }
        }

        public void LateUpdate()
        {
            this.cache.Clear();
            for (int id = 0, count = this.entities.Count; id < count; id++)
            {
                if (this.entities[id])
                {
                    this.cache.Add(id);
                }
            }

            var entityCount = this.cache.Count;

            for (int i = 0, count = this.lateUpdateSystems.Count; i < count; i++)
            {
                var system = this.lateUpdateSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var id = this.cache[e];
                    system.LateUpdate(id);
                }
            }
        }

        #endregion

        #region Declare

        public void DeclareComponent<T>() where T : struct
        {
            var pool = new EcsPool<T>();
            this.componentPools.Add(typeof(T), pool);
        }

        public void DeclareSystem<T>() where T : IEcsSystem, new()
        {
            var system = new T();
            this.allSystems.Add(system);

            if (system is IEcsUpdate updateSystem)
            {
                this.updateSystems.Add(updateSystem);
            }

            if (system is IEcsFixedUpdate fixedUpdateSystem)
            {
                this.fixedUpdateSystems.Add(fixedUpdateSystem);
            }

            if (system is IEcsLateUpdate lateUpdateSystem)
            {
                this.lateUpdateSystems.Add(lateUpdateSystem);
            }

#if UNITY_EDITOR
            if (system is IEcsDrawGizmos gizmosSystem)
            {
                this.gizmosSystems.Add(gizmosSystem);
            }
#endif
        }

        public void DeclareObserver<E, T>() where T : IEcsObserver<E>, new() where E : struct
        {
            var eventType = typeof(E);
            EcsEmitter<E> tEmitter;

            if (this.eventEmitters.TryGetValue(eventType, out var emitter))
            {
                tEmitter = (EcsEmitter<E>) emitter;
            }
            else
            {
                tEmitter = new EcsEmitter<E>(this.eventSink);
                this.eventEmitters.Add(eventType, tEmitter);
            }

            tEmitter.AddObserver(new T());
        }

        public void DeclareExternalServices(IEnumerable<object> services)
        {
            this.externalServices.AddRange(services);
        }

        public void DeclareExternalService(object service)
        {
            this.externalServices.Add(service);
        }

        #endregion

        #region Install

        public void ResolveDependencies()
        {
            foreach (var system in this.allSystems)
            {
                this.Inject(system);
            }

            foreach (var eventPool in this.eventEmitters.Values)
            {
                foreach (var handler in eventPool.GetObservers())
                {
                    this.Inject(handler);
                }
            }
        }

        public void Inject(object target)
        {
            var type = target.GetType();

            var fields = ReflectionUtils.RetrieveFields(type);
            var fieldLength = fields.Count;
            for (var i = 0; i < fieldLength; i++)
            {
                var field = fields[i];
                var fieldType = field.FieldType;
                if (field.GetValue(target) == null)
                {
                    var dependency = this.ResolveDependency(fieldType);
                    field.SetValue(target, dependency);
                }
            }

            this.externalInjector?.Invoke(target);

            if (target is IEcsInjectable injectable)
            {
                injectable.Inject(this);
            }
        }

        private object ResolveDependency(Type type)
        {
            if (typeof(EcsWorld).IsAssignableFrom(type))
            {
                return this;
            }

            if (typeof(IEcsPool).IsAssignableFrom(type))
            {
                return this.ResolveComponentPool(type);
            }

            if (typeof(IEcsEmitter).IsAssignableFrom(type))
            {
                return this.ResolveEventEmitter(type);
            }

            if (this.ResolveService(type, out var service))
            {
                return service;
            }

            return null;
        }

        private object ResolveComponentPool(Type type)
        {
            var componentType = type.GenericTypeArguments[0];
            if (this.componentPools.TryGetValue(componentType, out var pool))
            {
                return pool;
            }

            throw new Exception($"Component pool {componentType.Name} is not found!");
        }

        private object ResolveEventEmitter(Type type)
        {
            var eventType = type.GenericTypeArguments[0];
            if (this.eventEmitters.TryGetValue(eventType, out var emitter))
            {
                return emitter;
            }

            throw new Exception($"Event emitter {eventType.Name} is not found!");
        }

        private bool ResolveService(Type type, out object result)
        {
            foreach (var service in this.externalServices)
            {
                if (type.IsInstanceOfType(service))
                {
                    result = service;
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion

        public void Subscribe<T>(int entity, Action<T> listener) where T : struct
        {
            var eventType = typeof(T);
            if (!this.eventEmitters.TryGetValue(eventType, out var emitter))
            {
                emitter = new EcsEmitter<T>(this.eventSink);
                this.eventEmitters.Add(eventType, emitter);
            }

            var tEmitter = (EcsEmitter<T>) emitter;
            tEmitter.Subscribe(entity, listener);
        }

        public void Subscribe<T>(int entity, IEcsObserver<T> observer) where T : struct
        {
            var eventType = typeof(T);
            if (!this.eventEmitters.TryGetValue(eventType, out var emitter))
            {
                emitter = new EcsEmitter<T>(this.eventSink);
                this.eventEmitters.Add(eventType, emitter);
            }

            emitter.Subscribe(entity, observer);
        }
        
        public void Subscribe(int entity, Type eventType, IEcsObserver listener)
        {
            this.Inject(listener);

            if (!this.eventEmitters.TryGetValue(eventType, out var emitter))
            {
                var genericEmitter = typeof(EcsEmitter<>).MakeGenericType(eventType);
                emitter = (IEcsEmitter) Activator.CreateInstance(genericEmitter, this.eventSink);
                this.eventEmitters.Add(eventType, emitter);
            }
            
            emitter.Subscribe(entity, listener);
        }
        
        public void Unsubscribe(int entity, Type eventType, IEcsObserver listener)
        {
            if (this.eventEmitters.TryGetValue(eventType, out var emitter))
            {
                emitter.Unsubscribe(entity, listener);
            }
        }

        public void Unsubscribe<T>(int entity, Action<T> listener) where T : struct
        {
            if (this.eventEmitters.TryGetValue(typeof(T), out var emitter))
            {
                var tEmitter = (EcsEmitter<T>) emitter;
                tEmitter.Unsubscribe(entity, listener);
            }
        }

#if UNITY_EDITOR

        private readonly List<IEcsDrawGizmos> gizmosSystems = new();

        public void OnDrawGizmos()
        {
            this.cache.Clear();
            for (int id = 0, count = this.entities.Count; id < count; id++)
            {
                if (this.entities[id])
                {
                    this.cache.Add(id);
                }
            }

            var entityCount = this.cache.Count;

            for (int i = 0, count = this.gizmosSystems.Count; i < count; i++)
            {
                var system = this.gizmosSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var id = this.cache[e];
                    system.OnDrawGizmos(id);
                }
            }
        }

#endif
    }
}
