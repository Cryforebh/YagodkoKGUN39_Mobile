using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct TakeDamageEvent
    {
        public int sourceId;
        public int damage;
        public DamageType damageType;
    }
}