using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct CombatComponent
    {
        public int damage;
        public float minDistance;
        
        public float animationTime;
        public float timeBetweenAttack;
        public DamageType damageType;
    }
}