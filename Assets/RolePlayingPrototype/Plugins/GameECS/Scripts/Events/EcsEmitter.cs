using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameECS
{
    public sealed class EcsEmitter<T> : IEcsEmitter where T : struct
    {
        private readonly IEcsEventSink eventSink;
        private readonly List<IEcsObserver<T>> observers = new();
        private readonly Dictionary<int, Listener> entityListeners = new();

        public EcsEmitter(IEcsEventSink eventSink = null)
        {
            this.eventSink = eventSink;
        }

        public void SendEvent(int entity, T @event)
        {
            this.eventSink?.Publish(entity, @event);

            for (int i = 0, count = this.observers.Count; i < count; i++)
            {
                var observer = this.observers[i];
                observer.Handle(entity, @event);
            }
            
            if (this.entityListeners.TryGetValue(entity, out var listener))
            {
                listener.Invoke(entity, @event);
            }
        }

        internal void AddObserver(IEcsObserver<T> observer)
        {
            this.observers.Add(observer);
        }

        IEnumerable<object> IEcsEmitter.GetObservers()
        {
            return this.observers;
        }

        void IEcsEmitter.Subscribe(int entity, IEcsObserver observer)
        {
            if (observer is not IEcsObserver<T> tObserver)
            {
                return;
            }

            if (!this.entityListeners.TryGetValue(entity, out var listener))
            {
                listener = new Listener();
                this.entityListeners.Add(entity, listener);
            }

            listener.observers.Add(tObserver);
        }

        void IEcsEmitter.Unsubscribe(int entity, IEcsObserver observer)
        {
            if (observer is not IEcsObserver<T> tObserver)
            {
                return;
            }
            
            if (this.entityListeners.TryGetValue(entity, out var listener))
            {
                listener.observers.Remove(tObserver);
            }
        }

        internal void Subscribe(int entity, Action<T> callback)
        {
            if (!this.entityListeners.TryGetValue(entity, out var listener))
            {
                listener = new Listener();
                this.entityListeners.Add(entity, listener);
            }
            
            listener.onEvent += callback;
        }
        
        internal void Unsubscribe(int entity, Action<T> callback)
        {
            if (this.entityListeners.TryGetValue(entity, out var listener))
            {
                listener.onEvent -= callback;
            }
        }
        
        private sealed class Listener
        {
            internal event Action<T> onEvent;
            
            internal readonly List<IEcsObserver<T>> observers = new();
            private readonly List<IEcsObserver<T>> cache = new();

            public void Invoke(int entity, T @event)
            {
                this.onEvent?.Invoke(@event);
                
                this.cache.Clear();
                this.cache.AddRange(this.observers);

                for (int i = 0, count = this.cache.Count; i < count; i++)
                {
                    var observer = this.cache[i];
                    observer.Handle(entity, @event);
                }
            }
        }
    }
}
