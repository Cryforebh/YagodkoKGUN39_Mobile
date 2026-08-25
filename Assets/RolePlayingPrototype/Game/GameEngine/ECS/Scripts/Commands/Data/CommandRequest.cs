using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct CommandRequest
    {
        public CommandType type;
        public CommandStatus status;
        public object args;

        public bool Equals(CommandRequest other)
        {
            return type == other.type && Equals(args, other.args);
        }

        public override bool Equals(object obj)
        {
            return obj is CommandRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) type, args);
        }
    }
}