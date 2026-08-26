using Game.GameEngine.Ecs;
using UnityEngine;
using UnityEngine.Serialization;

namespace SampleProject
{
    [CreateAssetMenu(
        fileName = "CharacterConfig",
        menuName = "Gameplay/New CharacterConfig"
    )]
    public sealed class CharacterConfig : ScriptableObject
    {
        [Header("Common")]
        [FormerlySerializedAs("radius")]
        public float Radius;
        
        [Header("HitPoints")]
        [FormerlySerializedAs("hitPoints")]
        public int HitPoints = 100;
        
        [Header("Movement")]
        [FormerlySerializedAs("moveSpeed")]
        public float MoveSpeed = 5.0f;

        [Header("Combat")]
        [FormerlySerializedAs("damage")]
        public int Damage = 1;
        [FormerlySerializedAs("minDistance")]
        public float MinDistance = 1.0f;
        [FormerlySerializedAs("animationTime")]
        public float AnimationTime = 1.4f;
        [FormerlySerializedAs("timeBetweenAttack")]
        public float TimeBetweenAttack = 0.8f;
        [FormerlySerializedAs("damageType")]
        public DamageType DamageType = DamageType.MELEE;
    }
}
