using System;

namespace Game.GameEngine.Ecs
{
    public enum TeamId
    {
        Player = 0,
        Enemy = 1
    }

    [Serializable]
    public struct TeamComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("value")]         public TeamId Value;
    }

    public struct SelectedComponent
    {
    }
}
