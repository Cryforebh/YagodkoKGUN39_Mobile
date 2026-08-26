using GameECS;

namespace Game.GameEngine.Ecs
{
    public struct HitEvent
    {
[UnityEngine.Serialization.FormerlySerializedAs("target")]         public EntityHandle Target;
[UnityEngine.Serialization.FormerlySerializedAs("damage")]         public int Damage;
[UnityEngine.Serialization.FormerlySerializedAs("damageType")]         public DamageType DamageType;
    }
}
