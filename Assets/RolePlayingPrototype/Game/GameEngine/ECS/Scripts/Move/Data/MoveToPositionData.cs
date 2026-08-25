using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct MoveToPositionData
    {
        public Vector3 destination;
        public float stoppingDistance;
        public bool isReached;
    }
}