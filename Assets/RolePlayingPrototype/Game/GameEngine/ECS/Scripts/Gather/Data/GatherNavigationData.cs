using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public struct GatherNavigationData
    {
        public Vector3 Destination;
        public Vector3[] Corners;
        public int Pointer;
        public float StoppingDistance;
    }
}
