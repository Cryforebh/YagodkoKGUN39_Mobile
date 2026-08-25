using GameECS;
using SampleProject.Base;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherResourceSystem : IEcsFixedUpdate
    {
        private const float GATHERING_DURATION = 5.0f;
        private const string RESOURCE_TYPE = "Minerals";
        private const int RESOURCE_AMOUNT = 5;

        private EcsPool<GatherTarget> targetResourcePool;
        private EcsPool<GatherState> gatherStatePool;
        private EcsPool<GatherDuration> gatherDurationPool;
        private EcsPool<ResourceBag> resourceBagPool;

        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;

        private EcsWorld world;
        private CommandCenterEntity commandCenter;

        [Inject]
        private void Construct(CommandCenterEntity center)
        {
            commandCenter = center;
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.targetResourcePool.HasComponent(entity))
            {
                return;
            }

            ref var state = ref this.gatherStatePool.GetComponent(entity);
            if (state == GatherState.MOVE_TO_RESOURCE)
            {
                this.UpdateMoveToResourceState(entity);
            }
            else if (state == GatherState.GATHERING)
            {
                this.UpdateGatheringState(entity);
            }
            else if (state == GatherState.MOVE_TO_HOME)
            {
                this.UpdateMoveToBaseState(entity);
            }
        }

        private void UpdateMoveToResourceState(int entity)
        {
            if (!this.moveToPositionPool.HasComponent(entity))
            {
                this.AddMoveToResourceData(entity);
            }

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (!moveData.isReached)
            {
                return;
            }

            //Transitions:
            if (this.resourceBagPool.HasComponent(entity))
            {
                //Transit to MOVE_TO_BASE:
                this.SetMoveToHomeState(entity);
            }
            else
            {
                //Transit to GATHERING:
                this.SetGatheringState(entity);
            }
        }

        private void UpdateGatheringState(int entity)
        {
            if (this.gatherDurationPool.HasComponent(entity))
            {
                return;
            }

            this.resourceBagPool.SetComponent(entity, new ResourceBag
            {
                resourceType = RESOURCE_TYPE,
                resourceAmount = RESOURCE_AMOUNT
            });

            //Transit to move base:
            this.SetMoveToHomeState(entity);
        }

        private void UpdateMoveToBaseState(int entity)
        {
            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (!moveData.isReached)
            {
                return;
            }

            //Put resources to base...
            if (this.resourceBagPool.HasComponent(entity))
            {
                var gatherData = this.resourceBagPool.GetComponent(entity);
                this.resourceBagPool.RemoveComponent(entity);
                world.SendEvent(entity, new ResourceDeliveredEvent
                {
                    resourceType = gatherData.resourceType,
                    resourceAmount = gatherData.resourceAmount
                });
            }

            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;

            if (!this.world.IsEntityExists(resourceId)) //TODO: Find other resource...
            {
                //COMPLETE GATHERING IF RESOURCE NOT FOUND!!!
                this.StopGathering(entity);
                return;
            }

            this.SetMoveToResourceState(entity);
        }

        private void SetMoveToResourceState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_RESOURCE);
            this.AddMoveToResourceData(entity);
        }

        private void AddMoveToResourceData(int entity)
        {
            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;
            ref var resourceTransform = ref this.transformPool.GetComponent(resourceId);

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = resourceTransform.value.position,
                stoppingDistance = resourceTransform.radius
            });
        }

        private void SetGatheringState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.GATHERING);

            this.gatherDurationPool.SetComponent(entity, new GatherDuration
            {
                remainingTime = GATHERING_DURATION
            });
        }

        private void SetMoveToHomeState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_HOME);

            if (commandCenter == null)
            {
                //Command center is not found!
                this.StopGathering(entity);
                return;
            }

            ref var homeTransform = ref this.transformPool.GetComponent(commandCenter.Id);

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = homeTransform.value.position,
                stoppingDistance = homeTransform.radius
            });
        }

        private void StopGathering(int entity)
        {
            this.moveToPositionPool.RemoveComponent(entity);
            
            this.gatherStatePool.RemoveComponent(entity);
            this.targetResourcePool.RemoveComponent(entity);
            this.gatherDurationPool.RemoveComponent(entity);
        }
    }
}
