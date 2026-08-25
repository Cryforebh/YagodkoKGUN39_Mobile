namespace GameECS
{
    internal interface IEcsPool
    {
        bool HasComponent(int entity);

        void RemoveComponent(int entity);

        internal void AllocComponent();

        internal object GetRawComponent(int entity);

        internal void SetRawComponent(int entity, object data);
    }
}