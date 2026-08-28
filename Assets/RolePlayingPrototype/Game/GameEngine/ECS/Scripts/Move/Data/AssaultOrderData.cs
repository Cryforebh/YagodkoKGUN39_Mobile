using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public struct AssaultOrderData
    {
        public Vector3 Destination;
        public Vector3 ResolvedDestination;
        public float ArrivalDistance;
        public float NextPathAttemptTime;
        public float LastDistance;
        public float LastProgressTime;
        public bool HasResolvedDestination;
        public bool IsTrackingProgress;
    }
}
