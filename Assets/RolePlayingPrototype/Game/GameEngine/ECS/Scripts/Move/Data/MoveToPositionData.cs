using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct MoveToPositionData
    {
[UnityEngine.Serialization.FormerlySerializedAs("destination")]         public Vector3 Destination;
[UnityEngine.Serialization.FormerlySerializedAs("stoppingDistance")]         public float StoppingDistance;
[UnityEngine.Serialization.FormerlySerializedAs("isReached")]         public bool IsReached;
    }
}
