namespace GameECS
{
    public interface IEcsEventSink
    {
        void Publish<T>(int entity, T value) where T : struct;
    }
}
