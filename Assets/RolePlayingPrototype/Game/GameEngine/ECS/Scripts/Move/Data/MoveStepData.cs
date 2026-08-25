using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct MoveStepData
    {
        public Vector3 direction;
        public bool completed;
    }
}