using Game.GameEngine.Ecs;
using UnityEngine;
using UnityEngine.Serialization;

namespace SampleProject.ResourceObject
{
    public sealed class ResourceEntity : Entity
    {
        private const int GizmoSegments = 48;

        [SerializeField]
        [FormerlySerializedAs("resourceType")]
        private ResourceType _resourceType = ResourceType.Minerals;

        [SerializeField]
        [FormerlySerializedAs("amount")]
        private int _amount = 100;

        [SerializeField]
        [FormerlySerializedAs("amountPerGather")]
        private int _amountPerGather = 5;

        [SerializeField]
        [FormerlySerializedAs("gatheringDuration")]
        private float _gatheringDuration = 5.0f;

        [SerializeField, Min(0.1f)]
        [Tooltip("Радиус подхода к ресурсу в мировых единицах")]
        private float _gatherRadius = 1f;

        protected override void Init()
        {
            SetData(new TransformComponent
            {
                Value = transform,
                Radius = GetWorldGatherRadius()
            });

            SetData(new GameObjectComponent { Value = gameObject });
            SetData(new ResourceNodeComponent
            {
                Type = _resourceType,
                RemainingAmount = _amount,
                AmountPerGather = _amountPerGather,
                GatheringDuration = _gatheringDuration
            });
        }

        private float GetWorldGatherRadius()
        {
            return _gatherRadius;
        }

        private void OnValidate()
        {
            _gatherRadius = Mathf.Max(0.1f, _gatherRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.9f);
            var center = transform.position;
            var radius = GetWorldGatherRadius();
            var previous = center + Vector3.forward * radius;
            for (var i = 1; i <= GizmoSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / GizmoSegments;
                var current = center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }
    }
}
