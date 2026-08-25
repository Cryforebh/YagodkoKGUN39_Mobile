using System;

namespace GameECS
{
    public sealed class EcsPool<T> : IEcsPool where T : struct
    {
        private struct Component
        {
            public bool exists;
            public T value;
        }

        private Component[] components = new Component[256];
        
        private int size = 0;

        public ref T GetComponent(int entity)
        {
            ref var component = ref this.components[entity];
            return ref component.value;
        }
        
        public void SetComponent(int entity, T data)
        {
            ref var component = ref this.components[entity];
            component.exists = true;
            component.value = data;
        }

        public void SetComponent(int entity, ref T data)
        {
            ref var component = ref this.components[entity];
            component.exists = true;
            component.value = data;
        }

        public void RemoveComponent(int entity)
        {
            ref var component = ref this.components[entity];
            component.exists = false;
        }

        public bool HasComponent(int entity)
        {
            return this.components[entity].exists;
        }

        void IEcsPool.AllocComponent()
        {
            if (this.size + 1 >= this.components.Length)
            {
                Array.Resize(ref this.components, this.components.Length * 2);
            }

            this.components[this.size] = new Component
            {
                exists = false,
                value = default
            };
            
            this.size++;
        }

        object IEcsPool.GetRawComponent(int entity)
        {
            return this.components[entity].value;
        }

        void IEcsPool.SetRawComponent(int entity, object data)
        {
            this.components[entity] = new Component
            {
                exists = true,
                value = (T) data
            };
        }
    }
}