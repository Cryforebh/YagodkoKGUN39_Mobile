using Game.GameEngine.Ecs;
using UnityEngine;
using UnityEngine.Serialization;

namespace SampleProject.ResourceObject
{
    public sealed class ResourceEntity : Entity
    {
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

        protected override void Init()
        {
            SetData(new TransformComponent
            {
                Value = transform,
                Radius = 1
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
    }
}
