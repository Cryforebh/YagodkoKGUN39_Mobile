using System;

namespace GameECS
{
    [Serializable]
    public readonly struct EntityHandle : IEquatable<EntityHandle>
    {
        public static EntityHandle Invalid => new(-1, 0);

        public int Id { get; }
        public uint Generation { get; }

        public EntityHandle(int id, uint generation)
        {
            Id = id;
            Generation = generation;
        }

        public bool Equals(EntityHandle other)
        {
            return Id == other.Id && Generation == other.Generation;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Generation);
        }

        public static bool operator ==(EntityHandle left, EntityHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityHandle left, EntityHandle right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Id}:{Generation}";
        }
    }
}
