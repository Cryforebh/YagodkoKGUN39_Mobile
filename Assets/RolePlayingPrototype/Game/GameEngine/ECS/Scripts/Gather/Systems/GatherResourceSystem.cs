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
        private const float AcceptableDepotDetourFactor = 1.25f;
        private const float AcceptableDepotDetourDistance = 0.75f;

        private static readonly float[] DepotCandidateAngles =
        {
            0f,
            -45f,
            45f,
            -90f,
            90f,
            -135f,
            135f,
            180f
        };

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

            if (_resourceBagPool.HasComponent(entity))
            {
                this.SetMoveToHomeState(entity);
            }
            else
            {
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

            if (!_world.IsEntityExists(resource))
            {
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
            if (TrySetMoveToDepotDestination(entity, unitPosition, homeTransform.Value.position, homeTransform.Radius))
            {
                return;
            }

            var sampleDistance = Mathf.Max(NavMeshSampleDistance, homeTransform.Radius + ApproachStoppingDistance);
            SetMoveDestination(entity, unitPosition, homeTransform.Value.position, homeTransform.Radius, sampleDistance);
        }

        private bool TrySetMoveToDepotDestination(int entity, Vector3 unitPosition, Vector3 depotPosition, float depotRadius)
        {
            var approachRadius = Mathf.Max(0.1f, depotRadius);
            if (HorizontalDistance(unitPosition, depotPosition) <= approachRadius + ApproachStoppingDistance)
            {
                _navigationPool.RemoveComponent(entity);
                _moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    Destination = unitPosition,
                    StoppingDistance = ApproachStoppingDistance
                });
                return true;
            }

            depotPosition.y = unitPosition.y;
            var direction = Vector3.ProjectOnPlane(unitPosition - depotPosition, Vector3.up).normalized;
            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }

            var hasBestPath = false;
            var bestPath = default(NavigationPathResult);
            var bestLength = float.MaxValue;
            for (var i = 0; i < DepotCandidateAngles.Length; i++)
            {
                var candidateDirection = Quaternion.AngleAxis(DepotCandidateAngles[i], Vector3.up) * direction;
                var candidate = depotPosition + candidateDirection * approachRadius;
                if (!_navigation.TryBuildPath(unitPosition, candidate, NavMeshSampleDistance, out var path) || !path.IsComplete)
                {
                    continue;
                }

                var pathLength = CalculatePathLength(path.Corners);
                if (i == 0 && IsAcceptableDepotPath(path, pathLength))
                {
                    SetPreparedMoveDestination(entity, path, ApproachStoppingDistance);
                    return true;
                }

                if (pathLength < bestLength)
                {
                    bestLength = pathLength;
                    bestPath = path;
                    hasBestPath = true;
                }
            }

            if (!hasBestPath)
            {
                return false;
            }

            SetPreparedMoveDestination(entity, bestPath, ApproachStoppingDistance);
            return true;
        }

        private bool IsAcceptableDepotPath(NavigationPathResult path, float pathLength)
        {
            var directDistance = HorizontalDistance(path.Start, path.Destination);
            return pathLength <= directDistance * AcceptableDepotDetourFactor + AcceptableDepotDetourDistance;
        }

        private float CalculatePathLength(Vector3[] corners)
        {
            var length = 0f;
            for (var i = 1; i < corners.Length; i++)
            {
                length += HorizontalDistance(corners[i - 1], corners[i]);
            }

            return length;
        }

        private float HorizontalDistance(Vector3 left, Vector3 right)
        {
            var offset = right - left;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void SetPreparedMoveDestination(int entity, NavigationPathResult path, float stoppingDistance)
        {
            _navigationPool.RemoveComponent(entity);
            if (path.Corners.Length > 2)
            {
                _navigationPool.SetComponent(entity, new GatherNavigationData
                {
                    Destination = path.Destination,
                    Corners = path.Corners,
                    Pointer = 1,
                    StoppingDistance = stoppingDistance
                });
                SetCurrentNavigationPoint(entity);
                return;
            }

            _moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                Destination = path.Destination,
                StoppingDistance = stoppingDistance
            });
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
