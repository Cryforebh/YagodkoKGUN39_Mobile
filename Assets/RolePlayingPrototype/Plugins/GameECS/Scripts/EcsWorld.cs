using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameECS
{
    public sealed class EcsWorld
    {
        private readonly Dictionary<Type, IEcsPool> _componentPools = new();
        private readonly Dictionary<Type, IEcsEmitter> _eventEmitters = new();

        private readonly List<IEcsSystem> _allSystems = new();
        private readonly List<IEcsUpdate> _updateSystems = new();
        private readonly List<IEcsFixedUpdate> _fixedUpdateSystems = new();
        private readonly List<IEcsLateUpdate> _lateUpdateSystems = new();

        private readonly List<bool> _entities = new();
        private readonly List<uint> _generations = new();
        private readonly List<EntityHandle> _cache = new();

        private readonly List<object> _externalServices = new();
        private readonly IEcsEventSink _eventSink;
        private Action<object> _externalInjector;

        public EcsWorld(IEcsEventSink eventSink = null)
        {
            _eventSink = eventSink;
        }

        public void SetExternalInjector(Action<object> injector)
        {
            _externalInjector = injector;
        }

        #region Entities

        public int CreateEntity()
        {
            var id = 0;
            var count = _entities.Count;

            for (; id < count; id++)
            {
                if (!_entities[id])
                {
                    _entities[id] = true;
                    _generations[id] = NextGeneration(_generations[id]);
                    _eventSink?.Publish(id, new EntityCreatedEvent());
                    return id;
                }
            }

            id = count;
            _entities.Add(true);
            _generations.Add(1);

            foreach (var pool in _componentPools.Values)
            {
                pool.AllocComponent();
            }

            _eventSink?.Publish(id, new EntityCreatedEvent());
            return id;
        }

        public EntityHandle GetEntityHandle(int entity)
        {
            if (!this.IsEntityExists(entity))
            {
                return EntityHandle.Invalid;
            }

            return new EntityHandle(entity, _generations[entity]);
        }

        public bool IsEntityExists(int entity)
        {
            if (entity < 0 || entity >= _entities.Count)
            {
                return false;
            }

            return _entities[entity];
        }

        public bool IsEntityExists(EntityHandle entity)
        {
            return this.IsEntityExists(entity.Id) && _generations[entity.Id] == entity.Generation;
        }

        public void GetActiveEntities(List<EntityHandle> result)
        {
            result.Clear();
            for (var id = 0; id < _entities.Count; id++)
            {
                if (_entities[id])
                {
                    result.Add(this.GetEntityHandle(id));
                }
            }
        }

        public void DestroyEntity(EntityHandle entity)
        {
            if (!this.IsEntityExists(entity))
            {
                return;
            }

            this.DestroyEntity(entity.Id);
        }

        public void DestroyEntity(int entity)
        {
            if (!this.IsEntityExists(entity))
            {
                return;
            }

            _eventSink?.Publish(entity, new EntityDestroyedEvent());
            _entities[entity] = false;

            foreach (var componentPool in _componentPools.Values)
            {
                componentPool.RemoveComponent(entity);
            }

            foreach (var eventEmitter in _eventEmitters.Values)
            {
                eventEmitter.RemoveEntity(entity);
            }
        }

        #endregion

        #region Components

        public ref T GetComponent<T>(int entity) where T : struct
        {
            if (!_componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            var tComponentPool = (EcsPool<T>) componentPool;
            return ref tComponentPool.GetComponent(entity);
        }

        public void SetComponent<T>(int entity, ref T component) where T : struct
        {
            if (!_componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            var tComponentPool = (EcsPool<T>) componentPool;
            tComponentPool.SetComponent(entity, ref component);
        }

        public void RemoveComponent<T>(int entity) where T : struct
        {
            if (!_componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            componentPool.RemoveComponent(entity);
        }

        public bool HasComponent<T>(int entity) where T : struct
        {
            if (!_componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            return componentPool.HasComponent(entity);
        }

        internal object GetRawComponent(int entity, Type type)
        {
            var componentPool = _componentPools[type];
            return componentPool.GetRawComponent(entity);
        }

        internal void SetRawComponent(int entity, object data)
        {
            var componentPool = _componentPools[data.GetType()];
            componentPool.SetRawComponent(entity, data);
        }

        public List<object> GetRawComponents(int entityId)
        {
            var result = new List<object>();
            foreach (var pool in _componentPools.Values)
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
            if (!_componentPools.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            return (EcsPool<T>) componentPool;
        }

        #endregion

        #region Events

        public void SendEvent<T>(int entity, T @event) where T : struct
        {
            if (!_eventEmitters.TryGetValue(typeof(T), out var emitter))
            {
                emitter = new EcsEmitter<T>(_eventSink);
                _eventEmitters.Add(typeof(T), emitter);
            }

            var tEmitter = (EcsEmitter<T>) emitter;
            tEmitter.SendEvent(entity, @event);
        }

        public EcsEmitter<T> GetEmitter<T>() where T : struct
        {
            if (!_eventEmitters.TryGetValue(typeof(T), out var componentPool))
            {
                throw new Exception($"Component pool of type {typeof(T).Name} is not found!");
            }

            return (EcsEmitter<T>) componentPool;
        }

        #endregion

        #region Update

        public void Update()
        {
            _cache.Clear();
            for (int id = 0, count = _entities.Count; id < count; id++)
            {
                if (_entities[id])
                {
                    _cache.Add(this.GetEntityHandle(id));
                }
            }

            var entityCount = _cache.Count;

            for (int i = 0, count = _updateSystems.Count; i < count; i++)
            {
                var system = _updateSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var entity = _cache[e];
                    if (this.IsEntityExists(entity))
                    {
                        system.Update(entity.Id);
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            _cache.Clear();
            for (int id = 0, count = _entities.Count; id < count; id++)
            {
                if (_entities[id])
                {
                    _cache.Add(this.GetEntityHandle(id));
                }
            }

            var entityCount = _cache.Count;

            for (int i = 0, count = _fixedUpdateSystems.Count; i < count; i++)
            {
                var system = _fixedUpdateSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var entity = _cache[e];
                    if (this.IsEntityExists(entity))
                    {
                        system.FixedUpdate(entity.Id);
                    }
                }
            }
        }

        public void LateUpdate()
        {
            _cache.Clear();
            for (int id = 0, count = _entities.Count; id < count; id++)
            {
                if (_entities[id])
                {
                    _cache.Add(this.GetEntityHandle(id));
                }
            }

            var entityCount = _cache.Count;

            for (int i = 0, count = _lateUpdateSystems.Count; i < count; i++)
            {
                var system = _lateUpdateSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var entity = _cache[e];
                    if (this.IsEntityExists(entity))
                    {
                        system.LateUpdate(entity.Id);
                    }
                }
            }
        }

        #endregion

        #region Declare

        public void DeclareComponent<T>() where T : struct
        {
            var pool = new EcsPool<T>();
            _componentPools.Add(typeof(T), pool);
        }

        public void DeclareSystem<T>() where T : IEcsSystem, new()
        {
            var system = new T();
            _allSystems.Add(system);

            if (system is IEcsUpdate updateSystem)
            {
                _updateSystems.Add(updateSystem);
            }

            if (system is IEcsFixedUpdate fixedUpdateSystem)
            {
                _fixedUpdateSystems.Add(fixedUpdateSystem);
            }

            if (system is IEcsLateUpdate lateUpdateSystem)
            {
                _lateUpdateSystems.Add(lateUpdateSystem);
            }

#if UNITY_EDITOR
            if (system is IEcsDrawGizmos gizmosSystem)
            {
                _gizmosSystems.Add(gizmosSystem);
            }
#endif
        }

        public void DeclareObserver<E, T>() where T : IEcsObserver<E>, new() where E : struct
        {
            var eventType = typeof(E);
            EcsEmitter<E> tEmitter;

            if (_eventEmitters.TryGetValue(eventType, out var emitter))
            {
                tEmitter = (EcsEmitter<E>) emitter;
            }
            else
            {
                tEmitter = new EcsEmitter<E>(_eventSink);
                _eventEmitters.Add(eventType, tEmitter);
            }

            tEmitter.AddObserver(new T());
        }

        public void DeclareExternalServices(IEnumerable<object> services)
        {
            _externalServices.AddRange(services);
        }

        public void DeclareExternalService(object service)
        {
            _externalServices.Add(service);
        }

        #endregion

        #region Install

        public void ResolveDependencies()
        {
            foreach (var system in _allSystems)
            {
                this.Inject(system);
            }

            foreach (var eventPool in _eventEmitters.Values)
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

            _externalInjector?.Invoke(target);

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
            if (_componentPools.TryGetValue(componentType, out var pool))
            {
                return pool;
            }

            throw new Exception($"Component pool {componentType.Name} is not found!");
        }

        private object ResolveEventEmitter(Type type)
        {
            var eventType = type.GenericTypeArguments[0];
            if (_eventEmitters.TryGetValue(eventType, out var emitter))
            {
                return emitter;
            }

            throw new Exception($"Event emitter {eventType.Name} is not found!");
        }

        private bool ResolveService(Type type, out object result)
        {
            foreach (var service in _externalServices)
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
            if (!_eventEmitters.TryGetValue(eventType, out var emitter))
            {
                emitter = new EcsEmitter<T>(_eventSink);
                _eventEmitters.Add(eventType, emitter);
            }

            var tEmitter = (EcsEmitter<T>) emitter;
            tEmitter.Subscribe(entity, listener);
        }

        public void Subscribe<T>(int entity, IEcsObserver<T> observer) where T : struct
        {
            var eventType = typeof(T);
            if (!_eventEmitters.TryGetValue(eventType, out var emitter))
            {
                emitter = new EcsEmitter<T>(_eventSink);
                _eventEmitters.Add(eventType, emitter);
            }

            emitter.Subscribe(entity, observer);
        }
        
        public void Subscribe(int entity, Type eventType, IEcsObserver listener)
        {
            this.Inject(listener);

            if (!_eventEmitters.TryGetValue(eventType, out var emitter))
            {
                var genericEmitter = typeof(EcsEmitter<>).MakeGenericType(eventType);
                emitter = (IEcsEmitter) Activator.CreateInstance(genericEmitter, _eventSink);
                _eventEmitters.Add(eventType, emitter);
            }
            
            emitter.Subscribe(entity, listener);
        }
        
        public void Unsubscribe(int entity, Type eventType, IEcsObserver listener)
        {
            if (_eventEmitters.TryGetValue(eventType, out var emitter))
            {
                emitter.Unsubscribe(entity, listener);
            }
        }

        public void Unsubscribe<T>(int entity, Action<T> listener) where T : struct
        {
            if (_eventEmitters.TryGetValue(typeof(T), out var emitter))
            {
                var tEmitter = (EcsEmitter<T>) emitter;
                tEmitter.Unsubscribe(entity, listener);
            }
        }

#if UNITY_EDITOR

        private readonly List<IEcsDrawGizmos> _gizmosSystems = new();

        public void OnDrawGizmos()
        {
            _cache.Clear();
            for (int id = 0, count = _entities.Count; id < count; id++)
            {
                if (_entities[id])
                {
                    _cache.Add(GetEntityHandle(id));
                }
            }

            var entityCount = _cache.Count;

            for (int i = 0, count = _gizmosSystems.Count; i < count; i++)
            {
                var system = _gizmosSystems[i];

                for (var e = 0; e < entityCount; e++)
                {
                    var entity = _cache[e];
                    if (IsEntityExists(entity))
                    {
                        system.OnDrawGizmos(entity.Id);
                    }
                }
            }
        }

#endif

        private static uint NextGeneration(uint generation)
        {
            generation++;
            return generation == 0 ? 1 : generation;
        }
    }
}
