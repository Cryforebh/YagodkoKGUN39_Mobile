using Game.GameEngine.Ecs;
using GameECS;
using SampleProject.ResourceObject;
using UnityEngine;
using UnityEngine.AI;

namespace SampleProject
{
    public enum ContextCommandType
    {
        None,
        Move,
        Attack,
        Gather
    }

    public readonly struct ContextCommand
    {
        public readonly ContextCommandType Type;
        public readonly EntityHandle Target;
        public readonly Vector3 Position;

        public ContextCommand(ContextCommandType type, EntityHandle target, Vector3 position)
        {
            Type = type;
            Target = target;
            Position = position;
        }
    }

    public interface IContextCommandResolver
    {
        ContextCommand Resolve(RaycastHit hit, Vector3 point);
        bool TryResolveWalkablePosition(RaycastHit hit, Vector3 point, out Vector3 walkablePosition);
    }

    public sealed class ContextCommandResolver : IContextCommandResolver
    {
        private const float MinimumWalkableNormal = 0.65f;
        private const float NavMeshProbeDistance = 0.15f;
        private const float NavMeshHeightTolerance = 0.25f;
        private readonly EcsWorld _world;

        public ContextCommandResolver(EcsWorld world)
        {
            _world = world;
        }

        public ContextCommand Resolve(RaycastHit hit, Vector3 point)
        {
            var resource = hit.collider == null ? null : hit.collider.GetComponentInParent<ResourceEntity>();
            if (resource != null)
            {
                return new ContextCommand(ContextCommandType.Gather, resource.Handle, default);
            }

            var entity = hit.collider == null ? null : hit.collider.GetComponentInParent<Entity>();
            if (entity != null)
            {
                return CanAttack(entity.Handle) ? new ContextCommand(ContextCommandType.Attack, entity.Handle, default) : default;
            }

            return TryResolveWalkablePosition(hit, point, out var walkablePosition)
                ? new ContextCommand(ContextCommandType.Move, EntityHandle.Invalid, walkablePosition)
                : default;
        }

        public bool TryResolveWalkablePosition(RaycastHit hit, Vector3 point, out Vector3 walkablePosition)
        {
            walkablePosition = default;
            if (hit.collider != null && Vector3.Dot(hit.normal, Vector3.up) < MinimumWalkableNormal)
            {
                return false;
            }

            if (!NavMesh.SamplePosition(point, out var navMeshHit, NavMeshProbeDistance, NavMesh.AllAreas))
            {
                return false;
            }

            var horizontalOffset = Vector3.ProjectOnPlane(navMeshHit.position - point, Vector3.up);
            if (horizontalOffset.sqrMagnitude > NavMeshProbeDistance * NavMeshProbeDistance || Mathf.Abs(navMeshHit.position.y - point.y) > NavMeshHeightTolerance)
            {
                return false;
            }

            walkablePosition = navMeshHit.position;
            return true;
        }

        private bool CanAttack(EntityHandle target)
        {
            if (!_world.IsEntityExists(target) || !_world.HasComponent<TeamComponent>(target.Id))
            {
                return false;
            }

            return _world.GetComponent<TeamComponent>(target.Id).Value != TeamId.Player;
        }
    }
}
