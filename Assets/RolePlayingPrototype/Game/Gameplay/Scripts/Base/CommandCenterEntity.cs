using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject.Base
{
    public sealed class CommandCenterEntity : Entity, IResourceDepot
    {
        private const int GizmoSegments = 48;

        [SerializeField, Min(0.1f)]
        [Tooltip("Радиус доставки ресурсов в мировых единицах")]
        private float _deliveryRadius = 2.5f;

        protected override void Init()
        {
            SetData(new TransformComponent
            {
                Value = transform,
                Radius = _deliveryRadius
            });
        }

        private void OnValidate()
        {
            _deliveryRadius = Mathf.Max(0.1f, _deliveryRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.9f);
            var center = transform.position;
            var previous = center + Vector3.forward * _deliveryRadius;
            for (var i = 1; i <= GizmoSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / GizmoSegments;
                var current = center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * _deliveryRadius;
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }
    }
}
