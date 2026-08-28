using GameECS;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherResourceSystem : IEcsFixedUpdate
    {
        private const float ApproachStoppingDistance = 0.2f;
        private const float WaypointStoppingDistance = 0.6f;
        private const float NavMeshSampleDistance = 1f;

        private EcsPool<GatherTarget> _targetResourcePool;
        private EcsPool<GatherState> _gatherStatePool;
        private EcsPool<GatherDuration> _gatherDurationPool;
        private EcsPool<ResourceBag> _resourceBagPool;
        private EcsPool<ResourceNodeComponent> _resourceNodePool;
        private EcsPool<GatherNavigationData> _navigationPool;

        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsEmitter<SmoothRotateEvent> _rotateEmitter;

        private EcsWorld _world;
        private IResourceDepot _resourceDepot;
        private INavigationPathService _navigation;

        [Inject]
        private void Construct(IResourceDepot depot, INavigationPathService navigation)
        {
            _resourceDepot = depot;
            _navigation = navigation;
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!_targetResourcePool.HasComponent(entity))
            {
                return;
            }

            if (!_resourceBagPool.HasComponent(entity) && !IsResourceAvailable(entity))
            {
                StopGathering(entity);
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

        private bool IsResourceAvailable(int entity)
        {
            ref var target = ref _targetResourcePool.GetComponent(entity).Target;
            return _world.IsEntityExists(target) &&
                   _resourceNodePool.HasComponent(target.Id) &&
                   _resourceNodePool.GetComponent(target.Id).RemainingAmount > 0;
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

            if (TrySetNextNavigationPoint(entity))
            {
                return;
            }

            _navigationPool.RemoveComponent(entity);

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

            if (TrySetNextNavigationPoint(entity))
            {
                return;
            }

            _navigationPool.RemoveComponent(entity);

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
            ref var unitTransform = ref _transformPool.GetComponent(entity);
            var resourcePosition = resourceTransform.Value.position;
            var unitPosition = unitTransform.Value.position;
            var direction = Vector3.ProjectOnPlane(unitPosition - resourcePosition, Vector3.up).normalized;
            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }

            var destination = resourcePosition + direction * resourceTransform.Radius;
            if (NavMesh.SamplePosition(destination, out var hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                var hitOffset = Vector3.ProjectOnPlane(hit.position - resourcePosition, Vector3.up);
                if (hitOffset.sqrMagnitude >= resourceTransform.Radius * resourceTransform.Radius)
                {
                    destination = hit.position;
                }
            }

            SetMoveDestination(entity, unitPosition, destination, ApproachStoppingDistance, NavMeshSampleDistance);
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

            var unitPosition = _transformPool.GetComponent(entity).Value.position;
            var sampleDistance = Mathf.Max(NavMeshSampleDistance, homeTransform.Radius + ApproachStoppingDistance);
            SetMoveDestination(entity, unitPosition, homeTransform.Value.position, homeTransform.Radius, sampleDistance);
        }

        private void SetMoveDestination(int entity, Vector3 start, Vector3 destination, float stoppingDistance, float sampleDistance)
        {
            _navigationPool.RemoveComponent(entity);
            if (_navigation.TryBuildPath(start, destination, sampleDistance, out var path) && path.IsComplete && path.Corners.Length > 2)
            {
                _navigationPool.SetComponent(entity, new GatherNavigationData
                {
                    Destination = destination,
                    Corners = path.Corners,
                    Pointer = 1,
                    StoppingDistance = stoppingDistance
                });
                SetCurrentNavigationPoint(entity);
                return;
            }

            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = destination,
                StoppingDistance = stoppingDistance
            });
        }

        private bool TrySetNextNavigationPoint(int entity)
        {
            if (!_navigationPool.HasComponent(entity))
            {
                return false;
            }

            ref var route = ref _navigationPool.GetComponent(entity);
            if (route.Pointer >= route.Corners.Length - 1)
            {
                return false;
            }

            route.Pointer++;
            SetCurrentNavigationPoint(entity);
            return true;
        }

        private void SetCurrentNavigationPoint(int entity)
        {
            ref var route = ref _navigationPool.GetComponent(entity);
            var isLastCorner = route.Pointer == route.Corners.Length - 1;
            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = isLastCorner ? route.Destination : route.Corners[route.Pointer],
                StoppingDistance = isLastCorner ? route.StoppingDistance : WaypointStoppingDistance
            });
        }

        private void StopGathering(int entity)
        {
            _moveToPositionPool.RemoveComponent(entity);
            
            _gatherStatePool.RemoveComponent(entity);
            _targetResourcePool.RemoveComponent(entity);
            _gatherDurationPool.RemoveComponent(entity);
            _navigationPool.RemoveComponent(entity);
        }
    }
}
