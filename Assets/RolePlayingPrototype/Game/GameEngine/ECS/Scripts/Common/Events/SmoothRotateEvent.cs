using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct SmoothRotateEvent
    {
        public Vector3 direction;
    }
}