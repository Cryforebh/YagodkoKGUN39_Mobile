using System;
using UnityEngine;
using UnityEngine.AI;

namespace SampleProject
{
    public sealed class RecruitmentBuilding : MonoBehaviour
    {
        [Header("Recruitment")]
        [SerializeField] private RecruitmentDefinition[] _availableUnits = Array.Empty<RecruitmentDefinition>();

        [Header("Spawn and rally")]
        [SerializeField, Tooltip("Exact point where a recruited unit appears before moving to the rally point.")] private Transform _spawnPoint;
        [SerializeField, Tooltip("Default rally point. It can be moved at runtime with the rally point command.")] private Transform _rallyPoint;
        [SerializeField, Tooltip("Optional parent for recruited units. When empty, the building's parent is used.")] private Transform _unitsParent;
        [SerializeField, Min(0.1f)] private float _navMeshSearchDistance = 1.5f;
        [SerializeField, Min(0.5f)] private float _rallySpacing = 1.2f;

        private int _nextRallySlot;
        private Vector3 _runtimeRallyPoint;
        private bool _hasRuntimeRallyPoint;

        public RecruitmentDefinition[] AvailableUnits => _availableUnits;
        public Transform UnitsParent => _unitsParent != null ? _unitsParent : transform.parent;
        public Vector3 SpawnPointPosition => (_spawnPoint != null ? _spawnPoint : transform).position;

        public bool Offers(RecruitmentDefinition definition)
        {
            if (definition == null || _availableUnits == null)
            {
                return false;
            }

            for (var index = 0; index < _availableUnits.Length; index++)
            {
                if (_availableUnits[index] == definition)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSpawnPose(out Vector3 position, out Quaternion rotation)
        {
            var point = _spawnPoint != null ? _spawnPoint : transform;
            rotation = point.rotation;
            if (NavMesh.SamplePosition(point.position, out var hit, _navMeshSearchDistance, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }

            position = default;
            return false;
        }

        public bool TryGetRallyPoint(out Vector3 position)
        {
            var point = GetRallyPointPosition();
            if (NavMesh.SamplePosition(point, out var hit, _navMeshSearchDistance, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }

            position = default;
            return false;
        }

        public bool TryGetNextRallyDestination(out Vector3 position)
        {
            if (!TryGetRallyPoint(out var point))
            {
                position = default;
                return false;
            }

            const int maximumAttempts = 32;
            const float slotProbeDistance = 0.3f;
            for (var attempt = 0; attempt < maximumAttempts; attempt++)
            {
                var gridOffset = GetSpiralOffset(_nextRallySlot + attempt);
                var offset = new Vector2(gridOffset.x, gridOffset.y) * _rallySpacing;
                var candidate = point + new Vector3(offset.x, 0f, offset.y);
                if (!NavMesh.SamplePosition(candidate, out var hit, slotProbeDistance, NavMesh.AllAreas))
                {
                    continue;
                }

                _nextRallySlot += attempt + 1;
                position = hit.position;
                return true;
            }

            position = default;
            return false;
        }

        public void SetRallyPoint(Vector3 position)
        {
            _runtimeRallyPoint = position;
            _hasRuntimeRallyPoint = true;
            if (_rallyPoint != null)
            {
                _rallyPoint.position = position;
            }

            _nextRallySlot = 0;
        }

        private Vector3 GetRallyPointPosition()
        {
            if (_rallyPoint != null)
            {
                return _rallyPoint.position;
            }

            if (_hasRuntimeRallyPoint)
            {
                return _runtimeRallyPoint;
            }

            return SpawnPointPosition;
        }

        private void OnValidate()
        {
            _navMeshSearchDistance = Mathf.Max(0.1f, _navMeshSearchDistance);
            _rallySpacing = Mathf.Max(0.5f, _rallySpacing);
        }

        private static Vector2Int GetSpiralOffset(int index)
        {
            if (index <= 0)
            {
                return Vector2Int.zero;
            }

            var layer = Mathf.CeilToInt((Mathf.Sqrt(index + 1) - 1f) * 0.5f);
            var sideLength = layer * 2;
            var maximumIndex = (layer * 2 + 1) * (layer * 2 + 1) - 1;
            var distanceFromMaximum = maximumIndex - index;
            if (distanceFromMaximum < sideLength)
            {
                return new Vector2Int(layer - distanceFromMaximum, -layer);
            }

            distanceFromMaximum -= sideLength;
            if (distanceFromMaximum < sideLength)
            {
                return new Vector2Int(-layer, -layer + distanceFromMaximum);
            }

            distanceFromMaximum -= sideLength;
            if (distanceFromMaximum < sideLength)
            {
                return new Vector2Int(-layer + distanceFromMaximum, layer);
            }

            distanceFromMaximum -= sideLength;
            return new Vector2Int(layer, layer - distanceFromMaximum);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var point = _spawnPoint != null ? _spawnPoint : transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(point.position, 0.5f);
            Gizmos.DrawRay(point.position, point.forward * 2f);

            var rallyPoint = _rallyPoint != null ? _rallyPoint : point;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rallyPoint.position, 0.65f);
            Gizmos.DrawLine(point.position, rallyPoint.position);
        }
#endif
    }
}
