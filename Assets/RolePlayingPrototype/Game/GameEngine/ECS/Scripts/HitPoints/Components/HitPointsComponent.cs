using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct HitPointsComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("max")]         public int Max;
[UnityEngine.Serialization.FormerlySerializedAs("current")]         public int Current;
    }
}