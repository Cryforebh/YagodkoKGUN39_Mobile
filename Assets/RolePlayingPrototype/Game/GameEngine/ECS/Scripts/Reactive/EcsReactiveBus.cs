using System;
using System.Collections.Generic;
using GameECS;
using UniRx;

namespace Game.GameEngine.Ecs
{
    public readonly struct EcsEvent<T> where T : struct
    {
        public int Entity { get; }
        public T Value { get; }

        public EcsEvent(int entity, T value)
        {
            Entity = entity;
            Value = value;
        }
    }

    public interface IEcsEventStream
    {
        IObservable<EcsEvent<T>> Observe<T>() where T : struct;
    }

    public sealed class EcsReactiveBus : IEcsEventSink, IEcsEventStream, IDisposable
    {
        private readonly Dictionary<Type, object> _subjects = new();

        public IObservable<EcsEvent<T>> Observe<T>() where T : struct
        {
            return GetSubject<T>();
        }

        public void Publish<T>(int entity, T value) where T : struct
        {
            GetSubject<T>().OnNext(new EcsEvent<T>(entity, value));
        }

        public void Dispose()
        {
            foreach (var subject in _subjects.Values)
            {
                ((IDisposable)subject).Dispose();
            }

            _subjects.Clear();
        }

        private Subject<EcsEvent<T>> GetSubject<T>() where T : struct
        {
            var type = typeof(T);
            if (!_subjects.TryGetValue(type, out var subject))
            {
                subject = new Subject<EcsEvent<T>>();
                _subjects.Add(type, subject);
            }

            return (Subject<EcsEvent<T>>)subject;
        }
    }
}
