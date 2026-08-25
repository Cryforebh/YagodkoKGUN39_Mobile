namespace Game.GameEngine.Ecs
{
    public enum CommandType
    {
        UNDEFINED = -1,
        MOVE_TO_POSITION = 0,
        ATTACK_TARGET = 1,
        GATHER_RESOURCE = 2,
        PATROL_BY_POINTS = 3
    }
}