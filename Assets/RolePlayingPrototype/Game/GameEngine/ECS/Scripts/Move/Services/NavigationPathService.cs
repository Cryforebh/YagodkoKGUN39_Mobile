using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GameEngine.Ecs
{
    public interface INavigationPathService
    {
        bool TryBuildPath(Vector3 start, Vector3 destination, float sampleDistance, out NavigationPathResult result);
        bool HasDirectPath(Vector3 start, Vector3 destination, float sampleDistance);
    }

    public readonly struct NavigationPathResult
    {
        public Vector3 Start { get; }
        public Vector3 Destination { get; }
        public Vector3[] Corners { get; }
        public bool IsComplete { get; }

        public NavigationPathResult(Vector3 start, Vector3 destination, Vector3[] corners, bool isComplete)
        {
            Start = start;
            Destination = destination;
            Corners = corners;
            IsComplete = isComplete;
        }
    }

    public sealed class NavigationPathService : INavigationPathService
    {
        private static readonly ProfilerMarker CalculatePathMarker = new("Navigation.Path.Calculate");

        private const float StartSampleDistance = 0.5f;

        private readonly NavMeshPath _path = new();

        public bool TryBuildPath(Vector3 start, Vector3 destination, float sampleDistance, out NavigationPathResult result)
        {
            result = default;
            if (!NavMesh.SamplePosition(start, out var startHit, StartSampleDistance, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(destination, out var destinationHit, sampleDistance, NavMesh.AllAreas))
            {
                return false;
            }

            bool pathCalculated;
            using (CalculatePathMarker.Auto())
            {
                pathCalculated = NavMesh.CalculatePath(startHit.position, destinationHit.position, NavMesh.AllAreas, _path);
            }

            if (!pathCalculated || _path.status == NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            var corners = _path.corners;
            if (corners.Length < 2)
            {
                return false;
            }

            result = new NavigationPathResult(startHit.position, destinationHit.position, corners, _path.status == NavMeshPathStatus.PathComplete);
            return true;
        }

        public bool HasDirectPath(Vector3 start, Vector3 destination, float sampleDistance)
        {
            if (!NavMesh.SamplePosition(start, out var startHit, StartSampleDistance, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(destination, out var destinationHit, sampleDistance, NavMesh.AllAreas))
            {
                return false;
            }

            return !NavMesh.Raycast(startHit.position, destinationHit.position, out _, NavMesh.AllAreas);
        }
    }
}
