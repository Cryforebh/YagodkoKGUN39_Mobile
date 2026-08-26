using System;
using GameECS;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct HitRequest
    {
[UnityEngine.Serialization.FormerlySerializedAs("target")]         public EntityHandle Target;
    }
}
