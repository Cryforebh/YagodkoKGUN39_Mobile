using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct PatrolData
    {
        public List<Vector3> points;
        public int pointer;
        
        public float stoppingDistance;

        public Vector3 GetCurrentPoint()
        {
            return this.points[this.pointer];
        }

        public void MoveNext()
        {
            this.pointer = (this.pointer + 1) % this.points.Count;
        }
    }
}