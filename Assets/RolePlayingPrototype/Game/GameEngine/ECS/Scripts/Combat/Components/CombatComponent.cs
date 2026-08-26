using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct CombatComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("damage")]         public int Damage;
[UnityEngine.Serialization.FormerlySerializedAs("minDistance")]         public float MinDistance;
[UnityEngine.Serialization.FormerlySerializedAs("animationTime")]         
        public float AnimationTime;
[UnityEngine.Serialization.FormerlySerializedAs("timeBetweenAttack")]         public float TimeBetweenAttack;
[UnityEngine.Serialization.FormerlySerializedAs("damageType")]         public DamageType DamageType;
    }
}