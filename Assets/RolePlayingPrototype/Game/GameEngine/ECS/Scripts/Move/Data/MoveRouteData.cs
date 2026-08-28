using System.Collections.Generic;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveRouteCommand
    {
        public Vector3 Destination;
        public IReadOnlyList<Vector3> Waypoints;
    }

    public struct MoveRouteData
    {
        public Vector3 Destination;
        public IReadOnlyList<Vector3> Waypoints;
        public int Pointer;
    }
}
