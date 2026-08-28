using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct PatrolData
    {
        public PatrolGroupState Group;
        public int TargetPoint;
        public float StoppingDistance;
    }
}
