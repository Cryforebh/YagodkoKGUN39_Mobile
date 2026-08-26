using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct TransformComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("value")]         public Transform Value;
[UnityEngine.Serialization.FormerlySerializedAs("radius")]         public float Radius;
    }
}