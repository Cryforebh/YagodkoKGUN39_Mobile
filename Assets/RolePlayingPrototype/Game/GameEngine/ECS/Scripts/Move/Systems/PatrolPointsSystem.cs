using System.Collections.Generic;
using GameECS;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class PatrolPointsSystem : IEcsFixedUpdate
    {
        private const float NavigationSampleDistance = 2f;
        private const float WaypointStoppingDistance = 0.6f;
        private const float MinimumProgressDistance = 0.2f;
        private const float StuckDuration = 0.5f;
        private const float RepathDelay = 0.75f;
        private const int MaximumFailedAttempts = 6;

        private readonly List<EntityHandle> _invalidMembers = new();
        private readonly List<FormationUnit> _formationUnits = new();
        private readonly Dictionary<EntityHandle, Vector3> _emptyFormation = new();
        private EcsPool<PatrolData> _patrolPool;
        private EcsPool<PatrolRouteComponent> _routePool;
        private EcsPool<PatrolNavigationData> _navigationPool;
        private EcsPool<MoveToPositionData> _movePool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<HitPointsComponent> _hitPointsPool;
        private EcsPool<CommandRequest> _commandPool;
        private EcsWorld _world;
        private INavigationPathService _navigation;
        private IFormationPlannerService _formationPlanner;

        [Inject]
        private void Construct(INavigationPathService navigation, IFormationPlannerService formationPlanner)
        {
            _navigation = navigation;
            _formationPlanner = formationPlanner;
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _patrolPool, _transformPool))
            {
                return;
            }

            ref var patrol = ref _patrolPool.GetComponent(entity);
            var group = patrol.Group;
            if (group == null || group.Points == null || group.Points.Count == 0)
            {
                _movePool.RemoveComponent(entity);
                _navigationPool.RemoveComponent(entity);
                return;
            }

            RemoveInvalidMembers(group);
            var handle = _world.GetEntityHandle(entity);
            if (!group.HasFormation)
            {
                BuildFormation(group);
            }

            if (!group.TryGetDestination(handle, out var targetPosition))
            {
                RemoveFromPatrol(entity, group, handle);
                return;
            }

            if (group.HasArrived(handle))
            {
                _movePool.RemoveComponent(entity);
                _navigationPool.RemoveComponent(entity);
                group.TryMoveNext();
                return;
            }

            if (patrol.TargetPoint != group.CurrentPoint)
            {
                patrol.TargetPoint = group.CurrentPoint;
                _navigationPool.RemoveComponent(entity);
                SetDestination(entity, targetPosition, patrol.StoppingDistance);
            }
            else if (!_movePool.HasComponent(entity))
            {
                if (_navigationPool.HasComponent(entity))
                {
                    ref var navigation = ref _navigationPool.GetComponent(entity);
                    if (navigation.FailedAttempts >= MaximumFailedAttempts)
                    {
                        RemoveFromPatrol(entity, group, handle);
                        return;
                    }

                    if (Time.fixedTime < navigation.NextRepathTime)
                    {
                        return;
                    }
                }

                SetDestination(entity, targetPosition, patrol.StoppingDistance);
                if (!_movePool.HasComponent(entity))
                {
                    return;
                }
            }

            ref var move = ref _movePool.GetComponent(entity);
            if (!move.IsReached)
            {
                if (IsStuck(entity, move.Destination))
                {
                    _movePool.RemoveComponent(entity);
                    SetDestination(entity, targetPosition, patrol.StoppingDistance, true);
                }
                return;
            }

            if (TrySetNextNavigationPoint(entity, patrol.StoppingDistance))
            {
                return;
            }

            _movePool.RemoveComponent(entity);
            _navigationPool.RemoveComponent(entity);
            group.MarkArrived(handle);
            group.TryMoveNext();
        }

        private void SetDestination(int entity, Vector3 destination, float stoppingDistance, bool failedToProgress = false)
        {
            var failedAttempts = 0;
            if (_navigationPool.HasComponent(entity))
            {
                failedAttempts = _navigationPool.GetComponent(entity).FailedAttempts;
                if (failedToProgress)
                {
                    failedAttempts++;
                }
            }

            var start = _transformPool.GetComponent(entity).Value.position;
            if (_navigation.TryBuildPath(start, destination, NavigationSampleDistance, out var path) && path.IsComplete)
            {
                _navigationPool.SetComponent(entity, new PatrolNavigationData
                {
                    Destination = destination,
                    Corners = path.Corners,
                    Pointer = 1,
                    LastProgressTime = Time.fixedTime,
                    FailedAttempts = failedAttempts
                });
                SetCurrentNavigationPoint(entity, stoppingDistance);
                return;
            }

            _movePool.RemoveComponent(entity);
            _navigationPool.SetComponent(entity, new PatrolNavigationData
            {
                Destination = destination,
                Corners = null,
                NextRepathTime = Time.fixedTime + RepathDelay,
                FailedAttempts = failedAttempts + (failedToProgress ? 0 : 1)
            });
        }

        private bool TrySetNextNavigationPoint(int entity, float stoppingDistance)
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
            SetCurrentNavigationPoint(entity, stoppingDistance);
            return true;
        }

        private void SetCurrentNavigationPoint(int entity, float stoppingDistance)
        {
            ref var route = ref _navigationPool.GetComponent(entity);
            var isLastCorner = route.Pointer == route.Corners.Length - 1;
            _movePool.SetComponent(entity, new MoveToPositionData
            {
                Destination = isLastCorner ? route.Destination : route.Corners[route.Pointer],
                StoppingDistance = isLastCorner ? stoppingDistance : WaypointStoppingDistance
            });
            route.LastDistance = HorizontalDistance(_transformPool.GetComponent(entity).Value.position, isLastCorner ? route.Destination : route.Corners[route.Pointer]);
            route.LastProgressTime = Time.fixedTime;
        }

        private bool IsStuck(int entity, Vector3 destination)
        {
            if (!_navigationPool.HasComponent(entity))
            {
                return false;
            }

            ref var route = ref _navigationPool.GetComponent(entity);
            var distance = HorizontalDistance(_transformPool.GetComponent(entity).Value.position, destination);
            if (distance + MinimumProgressDistance < route.LastDistance)
            {
                route.LastDistance = distance;
                route.LastProgressTime = Time.fixedTime;
                route.FailedAttempts = 0;
                return false;
            }

            return Time.fixedTime - route.LastProgressTime >= StuckDuration;
        }

        private float HorizontalDistance(Vector3 left, Vector3 right)
        {
            var offset = right - left;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void RemoveFromPatrol(int entity, PatrolGroupState group, EntityHandle handle)
        {
            group.Remove(handle);
            _movePool.RemoveComponent(entity);
            _navigationPool.RemoveComponent(entity);
            _routePool.RemoveComponent(entity);
            _commandPool.RemoveComponent(entity);
            group.TryMoveNext();
        }

        private void BuildFormation(PatrolGroupState group)
        {
            _formationUnits.Clear();
            var groupCenter = Vector3.zero;
            foreach (var member in group.Members)
            {
                if (!_world.IsEntityExists(member) || !_transformPool.HasComponent(member.Id))
                {
                    continue;
                }

                var position = _transformPool.GetComponent(member.Id).Value.position;
                _formationUnits.Add(new FormationUnit(member, position));
                groupCenter += position;
            }

            var count = _formationUnits.Count;
            if (count == 0)
            {
                group.SetFormation(_emptyFormation);
                return;
            }

            groupCenter /= count;
            var center = group.Points[group.CurrentPoint];
            var forward = Vector3.ProjectOnPlane(center - groupCenter, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            if (_formationPlanner.TryBuild(_formationUnits, center, forward, out var destinations))
            {
                group.SetFormation(destinations);
                return;
            }

            group.SetFormation(_emptyFormation);
        }

        private void RemoveInvalidMembers(PatrolGroupState group)
        {
            _invalidMembers.Clear();
            foreach (var member in group.Members)
            {
                if (!_world.IsEntityExists(member) || !_routePool.HasComponent(member.Id) || _routePool.GetComponent(member.Id).Group != group ||
                    _hitPointsPool.HasComponent(member.Id) && _hitPointsPool.GetComponent(member.Id).Current <= 0)
                {
                    _invalidMembers.Add(member);
                }
            }

            for (var i = 0; i < _invalidMembers.Count; i++)
            {
                group.Remove(_invalidMembers[i]);
            }
        }

    }
}
