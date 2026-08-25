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
        private const int UNDEFINED = -1;

        public int Id => id.Value;
        public IReadOnlyReactiveProperty<int> ReactiveId => id;

        private readonly ReactiveProperty<int> id = new(UNDEFINED);
        private EcsWorld world;

        [Inject]
        private void Construct(EcsWorld ecsWorld)
        {
            world = ecsWorld;
            if (isActiveAndEnabled && !IsExists())
            {
                Activate();
            }
        }

        private void OnEnable()
        {
            if (world != null && !IsExists())
            {
                Activate();
            }
        }

        private void OnDisable()
        {
            if (world != null && IsExists())
            {
                world.DestroyEntity(Id);
                id.Value = UNDEFINED;
            }
        }

        private void OnDestroy()
        {
            id.Dispose();
        }

        private void Activate()
        {
            id.Value = world.CreateEntity();
            Init();
        }

        protected virtual void Init()
        {
        }

        public bool IsExists()
        {
            return Id >= 0;
        }

        public ref T GetData<T>() where T : struct
        {
            return ref world.GetComponent<T>(Id);
        }

        public void SetData<T>(T component) where T : struct
        {
            world.SetComponent(Id, ref component);
        }

        public void RemoveData<T>() where T : struct
        {
            world.RemoveComponent<T>(Id);
        }

        public bool HasData<T>() where T : struct
        {
            return world.HasComponent<T>(Id);
        }

        public void SendEvent<T>(T data) where T : struct
        {
            world.SendEvent(Id, data);
        }

        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            world.Subscribe(Id, listener);
        }

        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            world.Unsubscribe(Id, listener);
        }

        public List<object> GetDataSet()
        {
            if (Id != UNDEFINED)
            {
                return world.GetRawComponents(Id);
            }

            return new List<object>();
        }
    }
}
