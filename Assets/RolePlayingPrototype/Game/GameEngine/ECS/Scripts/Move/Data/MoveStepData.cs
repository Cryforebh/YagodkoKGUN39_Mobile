using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct MoveStepData
    {
[UnityEngine.Serialization.FormerlySerializedAs("direction")]         public Vector3 Direction;
[UnityEngine.Serialization.FormerlySerializedAs("completed")]         public bool Completed;
    }
}