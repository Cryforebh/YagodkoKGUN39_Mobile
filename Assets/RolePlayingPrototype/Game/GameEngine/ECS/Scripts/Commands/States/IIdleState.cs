namespace Game.GameEngine.Ecs
{
    public interface IIdleState
    {
        void OnEnter(int entity);
        void OnUpdate(int entity);
        void OnExit(int entity);
    }
}