using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct CommandRequest
    {
[UnityEngine.Serialization.FormerlySerializedAs("type")]         public CommandType Type;
[UnityEngine.Serialization.FormerlySerializedAs("status")]         public CommandStatus Status;
[UnityEngine.Serialization.FormerlySerializedAs("args")]         public object Args;

        public bool Equals(CommandRequest other)
        {
            return Type == other.Type && Equals(Args, other.Args);
        }

        public override bool Equals(object obj)
        {
            return obj is CommandRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) Type, Args);
        }
    }
}