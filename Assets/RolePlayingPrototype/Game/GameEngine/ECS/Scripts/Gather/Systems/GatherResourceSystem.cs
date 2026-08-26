using GameECS;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherResourceSystem : IEcsFixedUpdate
    {
        private EcsPool<GatherTarget> _targetResourcePool;
        private EcsPool<GatherState> _gatherStatePool;
        private EcsPool<GatherDuration> _gatherDurationPool;
        private EcsPool<ResourceBag> _resourceBagPool;
        private EcsPool<ResourceNodeComponent> _resourceNodePool;

        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsEmitter<SmoothRotateEvent> _rotateEmitter;

        private EcsWorld _world;
        private IResourceDepot _resourceDepot;

        [Inject]
        private void Construct(IResourceDepot depot)
        {
            _resourceDepot = depot;
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_targetResourcePool.HasComponent(entity))
            {
                return;
            }

            ref var state = ref _gatherStatePool.GetComponent(entity);
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
            if (!_moveToPositionPool.HasComponent(entity))
            {
                this.AddMoveToResourceData(entity);
            }

            ref var moveData = ref _moveToPositionPool.GetComponent(entity);
            if (!moveData.IsReached)
            {
                return;
            }

            //Transitions:
            if (_resourceBagPool.HasComponent(entity))
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
            this.FaceResource(entity);

            if (_gatherDurationPool.HasComponent(entity))
            {
                return;
            }

            ref var target = ref _targetResourcePool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_resourceNodePool.HasComponent(target.Id))
            {
                StopGathering(entity);
                return;
            }

            ref var node = ref _resourceNodePool.GetComponent(target.Id);
            var gatheredAmount = Mathf.Min(node.AmountPerGather, node.RemainingAmount);
            node.RemainingAmount -= gatheredAmount;
            _resourceBagPool.SetComponent(entity, new ResourceBag { ResourceType = node.Type, ResourceAmount = gatheredAmount });

            if (node.RemainingAmount <= 0)
            {
                _world.SendEvent(target.Id, new DestroyEvent());
            }

            //Transit to move base:
            this.SetMoveToHomeState(entity);
        }

        private void UpdateMoveToBaseState(int entity)
        {
            ref var moveData = ref _moveToPositionPool.GetComponent(entity);
            if (!moveData.IsReached)
            {
                return;
            }

            //Put resources to base...
            if (_resourceBagPool.HasComponent(entity))
            {
                var gatherData = _resourceBagPool.GetComponent(entity);
                _resourceBagPool.RemoveComponent(entity);
                _world.SendEvent(entity, new ResourceDeliveredEvent
                {
                    ResourceType = gatherData.ResourceType,
                    ResourceAmount = gatherData.ResourceAmount
                });
            }

            ref var resource = ref _targetResourcePool.GetComponent(entity).Target;

            if (!_world.IsEntityExists(resource)) //TODO: Find other resource...
            {
                //COMPLETE GATHERING IF RESOURCE NOT FOUND!!!
                this.StopGathering(entity);
                return;
            }

            this.SetMoveToResourceState(entity);
        }

        private void SetMoveToResourceState(int entity)
        {
            _gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_RESOURCE);
            this.AddMoveToResourceData(entity);
        }

        private void AddMoveToResourceData(int entity)
        {
            ref var resource = ref _targetResourcePool.GetComponent(entity).Target;
            ref var resourceTransform = ref _transformPool.GetComponent(resource.Id);

            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = resourceTransform.Value.position,
                StoppingDistance = resourceTransform.Radius
            });
        }

        private void SetGatheringState(int entity)
        {
            ref var target = ref _targetResourcePool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_resourceNodePool.HasComponent(target.Id))
            {
                StopGathering(entity);
                return;
            }

            _gatherStatePool.SetComponent(entity, GatherState.GATHERING);
            ref var node = ref _resourceNodePool.GetComponent(target.Id);
            this.FaceResource(entity);

            _gatherDurationPool.SetComponent(entity, new GatherDuration
            {
                RemainingTime = node.GatheringDuration
            });
        }

        private void FaceResource(int entity)
        {
            ref var target = ref _targetResourcePool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_transformPool.HasComponent(entity) || !_transformPool.HasComponent(target.Id))
            {
                return;
            }

            var unitPosition = _transformPool.GetComponent(entity).Value.position;
            var resourcePosition = _transformPool.GetComponent(target.Id).Value.position;
            var direction = Vector3.ProjectOnPlane(resourcePosition - unitPosition, Vector3.up).normalized;
            if (direction != Vector3.zero)
            {
                _rotateEmitter.SendEvent(entity, new SmoothRotateEvent { Direction = direction });
            }
        }

        private void SetMoveToHomeState(int entity)
        {
            _gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_HOME);

            if (_resourceDepot == null || !_world.IsEntityExists(_resourceDepot.Handle))
            {
                this.StopGathering(entity);
                return;
            }

            ref var homeTransform = ref _transformPool.GetComponent(_resourceDepot.Handle.Id);

            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = homeTransform.Value.position,
                StoppingDistance = homeTransform.Radius
            });
        }

        private void StopGathering(int entity)
        {
            _moveToPositionPool.RemoveComponent(entity);
            
            _gatherStatePool.RemoveComponent(entity);
            _targetResourcePool.RemoveComponent(entity);
            _gatherDurationPool.RemoveComponent(entity);
        }
    }
}
