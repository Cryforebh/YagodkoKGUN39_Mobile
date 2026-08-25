using System;
using UniRx;

namespace Game.GameEngine.Ecs
{
    public static class EcsEventStreamExtensions
    {
        public static IObservable<T> ObserveEntity<T>(this IEcsEventStream stream, int entityId)
            where T : struct
        {
            return stream.Observe<T>()
                .Where(message => message.Entity == entityId)
                .Select(message => message.Value);
        }

        public static IObservable<T> ObserveEntity<T>(this IEcsEventStream stream, Entity entity)
            where T : struct
        {
            return stream.Observe<T>()
                .Where(message => entity != null && message.Entity == entity.Id)
                .Select(message => message.Value);
        }
    }
}
