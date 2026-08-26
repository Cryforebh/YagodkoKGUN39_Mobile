using System;
using GameECS;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct TakeDamageEvent
    {
[UnityEngine.Serialization.FormerlySerializedAs("source")]         public EntityHandle Source;
[UnityEngine.Serialization.FormerlySerializedAs("damage")]         public int Damage;
[UnityEngine.Serialization.FormerlySerializedAs("damageType")]         public DamageType DamageType;
    }
}
