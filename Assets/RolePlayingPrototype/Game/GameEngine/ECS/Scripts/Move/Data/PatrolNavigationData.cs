using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public struct PatrolNavigationData
    {
        public Vector3 Destination;
        public Vector3[] Corners;
        public int Pointer;
        public float LastDistance;
        public float LastProgressTime;
        public float NextRepathTime;
        public int FailedAttempts;
    }
}
