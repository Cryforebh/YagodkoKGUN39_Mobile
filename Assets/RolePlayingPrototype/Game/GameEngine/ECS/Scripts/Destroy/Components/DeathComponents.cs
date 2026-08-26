using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct DeathSettingsComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("duration")]         public float Duration;
    }

    public struct DeathAnimationComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("elapsedTime")]         public float ElapsedTime;
[UnityEngine.Serialization.FormerlySerializedAs("startRotation")]         public Quaternion StartRotation;
[UnityEngine.Serialization.FormerlySerializedAs("targetRotation")]         public Quaternion TargetRotation;
    }
}
