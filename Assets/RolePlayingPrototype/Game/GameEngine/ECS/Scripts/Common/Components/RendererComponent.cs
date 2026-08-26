using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct RendererComponent
    {
        [SerializeField]
[UnityEngine.Serialization.FormerlySerializedAs("value")]         public Renderer Value;
    }
}