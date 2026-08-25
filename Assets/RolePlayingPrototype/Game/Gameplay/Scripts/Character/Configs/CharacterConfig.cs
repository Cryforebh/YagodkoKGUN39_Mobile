using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject
{
    [CreateAssetMenu(
        fileName = "CharacterConfig",
        menuName = "Gameplay/New CharacterConfig"
    )]
    public sealed class CharacterConfig : ScriptableObject
    {
        [Header("Common")]
        public float radius;
        
        [Header("HitPoints")]
        public int hitPoints = 100;
        
        [Header("Movement")]
        public float moveSpeed = 5.0f;

        [Header("Combat")]
        public int damage = 1;
        public float minDistance = 1.0f;
        public float animationTime = 1.4f;
        public float timeBetweenAttack = 0.8f;
        public DamageType damageType = DamageType.MELEE;
    }
}