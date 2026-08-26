using System;
using GameECS;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct AttackSlotComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("target")]         public EntityHandle Target;
[UnityEngine.Serialization.FormerlySerializedAs("slotIndex")]         public int SlotIndex;
[UnityEngine.Serialization.FormerlySerializedAs("slotCount")]         public int SlotCount;
    }
}
