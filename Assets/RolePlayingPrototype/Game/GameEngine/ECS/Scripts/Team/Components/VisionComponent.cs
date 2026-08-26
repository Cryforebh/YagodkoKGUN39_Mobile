using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct VisionComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("range")]         public float Range;
[UnityEngine.Serialization.FormerlySerializedAs("assistRange")]         public float AssistRange;
[UnityEngine.Serialization.FormerlySerializedAs("scanInterval")]         public float ScanInterval;
[UnityEngine.Serialization.FormerlySerializedAs("nextScanTime")]         public float NextScanTime;
    }
}
