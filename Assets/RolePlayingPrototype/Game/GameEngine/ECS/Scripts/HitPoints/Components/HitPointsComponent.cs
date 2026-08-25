using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct HitPointsComponent
    {
        public int max;
        public int current;
    }
}