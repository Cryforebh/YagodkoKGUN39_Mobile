using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct RendererComponent
    {
        [SerializeField]
        public Renderer value;
    }
}