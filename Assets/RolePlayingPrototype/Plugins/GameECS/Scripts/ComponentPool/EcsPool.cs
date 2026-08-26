using System;

namespace GameECS
{
    public sealed class EcsPool<T> : IEcsPool where T : struct
    {
        private struct Component
        {
[UnityEngine.Serialization.FormerlySerializedAs("exists")]             public bool Exists;
[UnityEngine.Serialization.FormerlySerializedAs("value")]             public T Value;
        }

        private Component[] _components = new Component[256];
        
        private int _size = 0;

        public ref T GetComponent(int entity)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!this.HasComponent(entity))
            {
                throw new InvalidOperationException(
                    $"Entity {entity} does not contain component {typeof(T).Name}.");
            }
#endif
            ref var component = ref _components[entity];
            return ref component.Value;
        }
        
        public void SetComponent(int entity, T data)
        {
            this.ValidateEntityIndex(entity);
            ref var component = ref _components[entity];
            component.Exists = true;
            component.Value = data;
        }

        public void SetComponent(int entity, ref T data)
        {
            this.ValidateEntityIndex(entity);
            ref var component = ref _components[entity];
            component.Exists = true;
            component.Value = data;
        }

        public void RemoveComponent(int entity)
        {
            if (entity < 0 || entity >= _size)
            {
                return;
            }

            ref var component = ref _components[entity];
            component.Exists = false;
            component.Value = default;
        }

        public bool HasComponent(int entity)
        {
            return entity >= 0 && entity < _size && _components[entity].Exists;
        }

        void IEcsPool.AllocComponent()
        {
            if (_size + 1 >= _components.Length)
            {
                Array.Resize(ref _components, _components.Length * 2);
            }

            _components[_size] = new Component
            {
                Exists = false,
                Value = default
            };
            
            _size++;
        }

        object IEcsPool.GetRawComponent(int entity)
        {
            return _components[entity].Value;
        }

        void IEcsPool.SetRawComponent(int entity, object data)
        {
            this.ValidateEntityIndex(entity);
            _components[entity] = new Component
            {
                Exists = true,
                Value = (T) data
            };
        }

        private void ValidateEntityIndex(int entity)
        {
            if (entity < 0 || entity >= _size)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(entity), entity, $"Entity index is outside the {typeof(T).Name} pool.");
            }
        }
    }

    public static class EcsFilter
    {
        public static bool Matches<T1, T2>(int entity, EcsPool<T1> pool1, EcsPool<T2> pool2) where T1 : struct where T2 : struct
        {
            return pool1.HasComponent(entity) && pool2.HasComponent(entity);
        }

        public static bool Matches<T1, T2, T3>(int entity, EcsPool<T1> pool1, EcsPool<T2> pool2, EcsPool<T3> pool3) where T1 : struct where T2 : struct where T3 : struct
        {
            return Matches(entity, pool1, pool2) && pool3.HasComponent(entity);
        }

        public static bool Matches<T1, T2, T3, T4>(int entity, EcsPool<T1> pool1, EcsPool<T2> pool2, EcsPool<T3> pool3, EcsPool<T4> pool4) where T1 : struct where T2 : struct where T3 : struct where T4 : struct
        {
            return Matches(entity, pool1, pool2, pool3) && pool4.HasComponent(entity);
        }

        public static bool Matches<T1, T2, T3, T4, T5>(int entity, EcsPool<T1> pool1, EcsPool<T2> pool2, EcsPool<T3> pool3, EcsPool<T4> pool4, EcsPool<T5> pool5) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
        {
            return Matches(entity, pool1, pool2, pool3, pool4) && pool5.HasComponent(entity);
        }
    }
}
