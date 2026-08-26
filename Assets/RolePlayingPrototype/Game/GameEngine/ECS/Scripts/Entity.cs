using System;
using System.Collections.Generic;
using GameECS;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    [DefaultExecutionOrder(-5000)]
    public class Entity : MonoBehaviour
    {
        public int Id => Handle.Id;
        public EntityHandle Handle => _handle.Value;
        public IReadOnlyReactiveProperty<EntityHandle> ReactiveHandle => _handle;

        private readonly ReactiveProperty<EntityHandle> _handle = new(EntityHandle.Invalid);
        private EcsWorld _world;

        [Inject]
        private void Construct(EcsWorld ecsWorld)
        {
            _world = ecsWorld;
            if (isActiveAndEnabled && !IsExists())
            {
                Activate();
            }
        }

        private void OnEnable()
        {
            if (_world != null && !IsExists())
            {
                Activate();
            }
        }

        private void OnDisable()
        {
            if (_world != null && IsExists())
            {
                _world.DestroyEntity(Handle);
                _handle.Value = EntityHandle.Invalid;
            }
        }

        private void OnDestroy()
        {
            _handle.Dispose();
        }

        private void Activate()
        {
            var entity = _world.CreateEntity();
            _handle.Value = _world.GetEntityHandle(entity);
            Init();
        }

        protected virtual void Init()
        {
        }

        public bool IsExists()
        {
            return _world != null && _world.IsEntityExists(Handle);
        }

        public ref T GetData<T>() where T : struct
        {
            return ref _world.GetComponent<T>(Id);
        }

        public void SetData<T>(T component) where T : struct
        {
            _world.SetComponent(Id, ref component);
        }

        public void RemoveData<T>() where T : struct
        {
            _world.RemoveComponent<T>(Id);
        }

        public bool HasData<T>() where T : struct
        {
            return _world.HasComponent<T>(Id);
        }

        public void SendEvent<T>(T data) where T : struct
        {
            _world.SendEvent(Id, data);
        }

        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            _world.Subscribe(Id, listener);
        }

        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            _world.Unsubscribe(Id, listener);
        }

        public List<object> GetDataSet()
        {
            if (IsExists())
            {
                return _world.GetRawComponents(Id);
            }

            return new List<object>();
        }
    }
}
