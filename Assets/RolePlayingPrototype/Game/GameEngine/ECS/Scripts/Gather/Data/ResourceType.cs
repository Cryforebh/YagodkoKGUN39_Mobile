namespace Game.GameEngine.Ecs
{
    public enum ResourceType
    {
        Minerals = 0,
        Wood = 1,
        Crystals = 2
    }

    public struct ResourceNodeComponent
    {
[UnityEngine.Serialization.FormerlySerializedAs("type")]         public ResourceType Type;
[UnityEngine.Serialization.FormerlySerializedAs("remainingAmount")]         public int RemainingAmount;
[UnityEngine.Serialization.FormerlySerializedAs("amountPerGather")]         public int AmountPerGather;
[UnityEngine.Serialization.FormerlySerializedAs("gatheringDuration")]         public float GatheringDuration;
    }
}
