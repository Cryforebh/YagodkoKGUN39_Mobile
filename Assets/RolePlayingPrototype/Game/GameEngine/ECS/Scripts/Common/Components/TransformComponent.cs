using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct TransformComponent
    {
        public Transform value;
        public float radius;
    }
}