using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct PatrolRouteComponent
    {
        public List<Vector3> Points;
    }
}
