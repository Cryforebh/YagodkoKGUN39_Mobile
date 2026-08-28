using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct MoveStepData
    {
        [UnityEngine.Serialization.FormerlySerializedAs("direction")]
        public Vector3 Direction;

        [NonSerialized]
        public Vector3 PreferredDirection;

        [UnityEngine.Serialization.FormerlySerializedAs("completed")]
        public bool Completed;
    }
}
