using UnityEngine;
using GameECS;

namespace Game.GameEngine.Ecs
{
    public struct AttackNavigationData
    {
        public EntityHandle Target;
        public Vector3 TargetPosition;
        public Vector3[] Corners;
        public int Pointer;
        public float NextRepathTime;
        public bool IsComplete;
    }
}
