using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameECS
{
    public sealed class EcsEmitter<T> : IEcsEmitter where T : struct
    {
        private readonly IEcsEventSink _eventSink;
        private readonly List<IEcsObserver<T>> _observers = new();
        private readonly Dictionary<int, Listener> _entityListeners = new();

        public EcsEmitter(IEcsEventSink eventSink = null)
        {
            _eventSink = eventSink;
        }

        public void SendEvent(int entity, T @event)
        {
            _eventSink?.Publish(entity, @event);

            for (int i = 0, count = _observers.Count; i < count; i++)
            {
                var observer = _observers[i];
                observer.Handle(entity, @event);
            }
            
            if (_entityListeners.TryGetValue(entity, out var listener))
            {
                listener.Invoke(entity, @event);
            }
        }

        internal void AddObserver(IEcsObserver<T> observer)
        {
            _observers.Add(observer);
        }

        IEnumerable<object> IEcsEmitter.GetObservers()
        {
            return _observers;
        }

        void IEcsEmitter.Subscribe(int entity, IEcsObserver observer)
        {
            if (observer is not IEcsObserver<T> tObserver)
            {
                return;
            }

            if (!_entityListeners.TryGetValue(entity, out var listener))
            {
                listener = new Listener();
                _entityListeners.Add(entity, listener);
            }

            listener.observers.Add(tObserver);
        }

        void IEcsEmitter.Unsubscribe(int entity, IEcsObserver observer)
        {
            if (observer is not IEcsObserver<T> tObserver)
            {
                return;
            }
            
            if (_entityListeners.TryGetValue(entity, out var listener))
            {
                listener.observers.Remove(tObserver);
            }
        }

        void IEcsEmitter.RemoveEntity(int entity)
        {
            _entityListeners.Remove(entity);
        }

        internal void Subscribe(int entity, Action<T> callback)
        {
            if (!_entityListeners.TryGetValue(entity, out var listener))
            {
                listener = new Listener();
                _entityListeners.Add(entity, listener);
            }
            
            listener.onEvent += callback;
        }
        
        internal void Unsubscribe(int entity, Action<T> callback)
        {
            if (_entityListeners.TryGetValue(entity, out var listener))
            {
                listener.onEvent -= callback;
            }
        }
        
        private sealed class Listener
        {
            internal event Action<T> onEvent;
            
            internal readonly List<IEcsObserver<T>> observers = new();
            private readonly List<IEcsObserver<T>> _cache = new();

            public void Invoke(int entity, T @event)
            {
                this.onEvent?.Invoke(@event);
                
                _cache.Clear();
                _cache.AddRange(this.observers);

                for (int i = 0, count = _cache.Count; i < count; i++)
                {
                    var observer = _cache[i];
                    observer.Handle(entity, @event);
                }
            }
        }
    }
}
