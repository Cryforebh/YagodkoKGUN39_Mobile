namespace GameECS
{
    public interface IEcsObserver
    {
    }

    public interface IEcsObserver<in T> : IEcsObserver where T : struct
    {
        void Handle(int entity, T @event);
    }
}