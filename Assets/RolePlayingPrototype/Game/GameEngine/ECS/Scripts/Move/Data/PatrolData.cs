using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct PatrolData
    {
[UnityEngine.Serialization.FormerlySerializedAs("points")]         public List<Vector3> Points;
[UnityEngine.Serialization.FormerlySerializedAs("pointer")]         public int Pointer;
[UnityEngine.Serialization.FormerlySerializedAs("stoppingDistance")]         
        public float StoppingDistance;

        public Vector3 GetCurrentPoint()
        {
            return this.Points[this.Pointer];
        }

        public void MoveNext()
        {
            this.Pointer = (this.Pointer + 1) % this.Points.Count;
        }
    }
}