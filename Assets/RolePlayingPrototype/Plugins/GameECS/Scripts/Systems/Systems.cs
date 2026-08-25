namespace GameECS
{
    public interface IEcsSystem
    {
    }

    public interface IEcsUpdate : IEcsSystem
    {
        void Update(int entity);
    }

    public interface IEcsFixedUpdate : IEcsSystem
    {
        void FixedUpdate(int entity);
    }

    public interface IEcsLateUpdate : IEcsSystem
    {
        void LateUpdate(int entity);
    }

    public interface IEcsDrawGizmos : IEcsSystem
    {
        void OnDrawGizmos(int entity);
    }
}