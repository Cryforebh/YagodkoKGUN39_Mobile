using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class EnemyDetectionSystem : IEcsFixedUpdate
    {
        private const float RetargetDistanceAdvantage = 0.5f;
        private readonly List<EntityHandle> _entities = new();
        private EcsPool<VisionComponent> _visionPool;
        private EcsPool<TeamComponent> _teamPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsPool<HitPointsComponent> _hitPointsPool;
        private EcsPool<CommandRequest> _commandPool;
        private EcsPool<AttackTarget> _attackTargetPool;
        private EcsPool<PatrolRouteComponent> _patrolRoutePool;
        private EcsPool<AssaultOrderData> _assaultOrderPool;
        private EcsWorld _world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _visionPool, _teamPool, _transformPool, _hitPointsPool))
            {
                return;
            }

            var hasCommand = _commandPool.HasComponent(entity);
            var hasPatrolRoute = _patrolRoutePool.HasComponent(entity);
            if (hasPatrolRoute && !hasCommand)
            {
                ResumePatrol(entity);
                return;
            }

            var isPatrolCommand = hasPatrolRoute && hasCommand && _commandPool.GetComponent(entity).Type == CommandType.PATROL_BY_POINTS;
            var isPatrolCombat = hasPatrolRoute && _attackTargetPool.HasComponent(entity);
            var canRetarget = _teamPool.GetComponent(entity).Value == TeamId.Enemy && _attackTargetPool.HasComponent(entity);
            var isAssaultMove = _assaultOrderPool.HasComponent(entity) && hasCommand &&
                                _commandPool.GetComponent(entity).Type == CommandType.MOVE_TO_POSITION;
            if (hasCommand && !canRetarget && !isPatrolCommand && !isPatrolCombat && !isAssaultMove)
            {
                return;
            }

            ref var vision = ref _visionPool.GetComponent(entity);
            if (Time.fixedTime < vision.NextScanTime)
            {
                return;
            }

            vision.NextScanTime = Time.fixedTime + Mathf.Max(0.05f, vision.ScanInterval);
            var target = FindNearestEnemy(entity, vision.Range);
            if (target == EntityHandle.Invalid)
            {
                if (isPatrolCombat)
                {
                    var currentTarget = _attackTargetPool.GetComponent(entity).Target;
                    var group = _patrolRoutePool.GetComponent(entity).Group;
                    if (group != null && !IsTargetVisibleToPatrolGroup(group, currentTarget) && TryFindPatrolGroupTarget(group, out var groupTarget))
                    {
                        StartPatrolCombat(entity, groupTarget);
                    }
                    else if (group == null || !IsTargetVisibleToPatrolGroup(group, currentTarget))
                    {
                        ResumePatrolGroup(entity, group);
                    }
                }

                return;
            }

            if (isPatrolCombat)
            {
                var currentTarget = _attackTargetPool.GetComponent(entity).Target;
                var group = _patrolRoutePool.GetComponent(entity).Group;
                if (group == null || !IsTargetVisibleToPatrolGroup(group, currentTarget))
                {
                    StartPatrolCombat(entity, target);
                }

                return;
            }

            if (canRetarget)
            {
                RetargetIfCloser(entity, target, vision.Range);
                return;
            }

            if (isPatrolCommand)
            {
                StartPatrolCombat(entity, target);
            }
            else if (isAssaultMove)
            {
                SetAttackCommand(entity, target);
            }

            AlertNearbyAllies(entity, target, vision.AssistRange);
        }

        private void StartPatrolCombat(int entity, EntityHandle target)
        {
            var route = _patrolRoutePool.GetComponent(entity);
            var group = route.Group;
            if (group == null)
            {
                SetAttackCommand(entity, target);
                return;
            }

            var team = _teamPool.GetComponent(entity).Value;
            foreach (var member in group.Members)
            {
                if (!_world.IsEntityExists(member) || !_patrolRoutePool.HasComponent(member.Id) ||
                    _patrolRoutePool.GetComponent(member.Id).Group != group || !_teamPool.HasComponent(member.Id) ||
                    _teamPool.GetComponent(member.Id).Value != team || !_hitPointsPool.HasComponent(member.Id) ||
                    _hitPointsPool.GetComponent(member.Id).Current <= 0)
                {
                    continue;
                }

                if (TryGetAssignedTarget(member.Id, out var assignedTarget) && IsTargetVisibleToPatrolGroup(group, assignedTarget))
                {
                    continue;
                }

                var memberTarget = EntityHandle.Invalid;
                if (_visionPool.HasComponent(member.Id))
                {
                    memberTarget = FindNearestEnemyFromBuffer(member.Id, _visionPool.GetComponent(member.Id).Range);
                }

                SetAttackCommand(member.Id, memberTarget != EntityHandle.Invalid ? memberTarget : target);
            }
        }

        private bool TryGetAssignedTarget(int entity, out EntityHandle target)
        {
            if (_commandPool.HasComponent(entity))
            {
                var command = _commandPool.GetComponent(entity);
                if (command.Type == CommandType.ATTACK_TARGET && command.Args is EntityHandle requestedTarget && IsValidTarget(requestedTarget))
                {
                    target = requestedTarget;
                    return true;
                }
            }

            if (_attackTargetPool.HasComponent(entity))
            {
                var currentTarget = _attackTargetPool.GetComponent(entity).Target;
                if (IsValidTarget(currentTarget))
                {
                    target = currentTarget;
                    return true;
                }
            }

            target = EntityHandle.Invalid;
            return false;
        }

        private bool IsValidTarget(EntityHandle target)
        {
            return _world.IsEntityExists(target) && _transformPool.HasComponent(target.Id) &&
                   _hitPointsPool.HasComponent(target.Id) && _hitPointsPool.GetComponent(target.Id).Current > 0;
        }

        private void SetAttackCommand(int entity, EntityHandle target)
        {
            _commandPool.SetComponent(entity, new CommandRequest
            {
                Type = CommandType.ATTACK_TARGET,
                Status = CommandStatus.IDLE,
                Args = target
            });
        }

        private bool IsTargetVisibleToPatrolGroup(PatrolGroupState group, EntityHandle target)
        {
            if (!IsValidTarget(target))
            {
                return false;
            }

            var targetPosition = _transformPool.GetComponent(target.Id).Value.position;
            foreach (var member in group.Members)
            {
                if (!_world.IsEntityExists(member) || !_visionPool.HasComponent(member.Id) ||
                    !_transformPool.HasComponent(member.Id) || !_hitPointsPool.HasComponent(member.Id) ||
                    _hitPointsPool.GetComponent(member.Id).Current <= 0)
                {
                    continue;
                }

                var range = _visionPool.GetComponent(member.Id).Range;
                var distance = (_transformPool.GetComponent(member.Id).Value.position - targetPosition).sqrMagnitude;
                if (distance <= range * range)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryFindPatrolGroupTarget(PatrolGroupState group, out EntityHandle target)
        {
            _world.GetActiveEntities(_entities);
            foreach (var member in group.Members)
            {
                if (!_world.IsEntityExists(member) || !_visionPool.HasComponent(member.Id) ||
                    !_transformPool.HasComponent(member.Id) || !_hitPointsPool.HasComponent(member.Id) ||
                    _hitPointsPool.GetComponent(member.Id).Current <= 0)
                {
                    continue;
                }

                target = FindNearestEnemyFromBuffer(member.Id, _visionPool.GetComponent(member.Id).Range);
                if (target != EntityHandle.Invalid)
                {
                    return true;
                }
            }

            target = EntityHandle.Invalid;
            return false;
        }

        private void ResumePatrolGroup(int entity, PatrolGroupState group)
        {
            if (group == null)
            {
                ResumePatrol(entity);
                return;
            }

            foreach (var member in group.Members)
            {
                if (!_world.IsEntityExists(member) || !_patrolRoutePool.HasComponent(member.Id) ||
                    _patrolRoutePool.GetComponent(member.Id).Group != group || !_hitPointsPool.HasComponent(member.Id) ||
                    _hitPointsPool.GetComponent(member.Id).Current <= 0)
                {
                    continue;
                }

                ResumePatrol(member.Id);
            }
        }

        private void ResumePatrol(int entity)
        {
            var points = _patrolRoutePool.GetComponent(entity).Points;
            if (points == null || points.Count == 0)
            {
                _patrolRoutePool.RemoveComponent(entity);
                return;
            }

            _commandPool.SetComponent(entity, new CommandRequest
            {
                Type = CommandType.PATROL_BY_POINTS,
                Status = CommandStatus.IDLE,
                Args = new List<Vector3>(points)
            });
        }

        private void RetargetIfCloser(int entity, EntityHandle candidate, float visionRange)
        {
            ref var currentTarget = ref _attackTargetPool.GetComponent(entity).Target;
            if (candidate == currentTarget)
            {
                return;
            }

            var position = _transformPool.GetComponent(entity).Value.position;
            var candidateDistance = Vector3.Distance(_transformPool.GetComponent(candidate.Id).Value.position, position);
            var currentDistance = float.MaxValue;
            if (_world.IsEntityExists(currentTarget) && _transformPool.HasComponent(currentTarget.Id) && _hitPointsPool.HasComponent(currentTarget.Id) && _hitPointsPool.GetComponent(currentTarget.Id).Current > 0)
            {
                currentDistance = Vector3.Distance(_transformPool.GetComponent(currentTarget.Id).Value.position, position);
            }

            if (currentDistance <= visionRange && candidateDistance + RetargetDistanceAdvantage >= currentDistance)
            {
                return;
            }

            currentTarget = candidate;
        }

        private EntityHandle FindNearestEnemy(int observer, float range)
        {
            _world.GetActiveEntities(_entities);
            return FindNearestEnemyFromBuffer(observer, range);
        }

        private EntityHandle FindNearestEnemyFromBuffer(int observer, float range)
        {
            var observerTeam = _teamPool.GetComponent(observer).Value;
            var observerPosition = _transformPool.GetComponent(observer).Value.position;
            var bestDistance = range * range;
            var result = EntityHandle.Invalid;

            for (var i = 0; i < _entities.Count; i++)
            {
                var candidate = _entities[i];
                if (candidate.Id == observer || !EcsFilter.Matches(candidate.Id, _teamPool, _transformPool, _hitPointsPool))
                {
                    continue;
                }

                if (_teamPool.GetComponent(candidate.Id).Value == observerTeam || _hitPointsPool.GetComponent(candidate.Id).Current <= 0)
                {
                    continue;
                }

                var distance = (_transformPool.GetComponent(candidate.Id).Value.position - observerPosition).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    result = candidate;
                }
            }

            return result;
        }

        private void AlertNearbyAllies(int observer, EntityHandle target, float assistRange)
        {
            var observerTeam = _teamPool.GetComponent(observer).Value;
            var observerPosition = _transformPool.GetComponent(observer).Value.position;
            var assistDistance = assistRange * assistRange;

            for (var i = 0; i < _entities.Count; i++)
            {
                var ally = _entities[i];
                if (!EcsFilter.Matches(ally.Id, _teamPool, _transformPool, _hitPointsPool))
                {
                    continue;
                }

                if (_commandPool.HasComponent(ally.Id))
                {
                    var command = _commandPool.GetComponent(ally.Id);
                    var isAssaultMove = _assaultOrderPool.HasComponent(ally.Id) && command.Type == CommandType.MOVE_TO_POSITION;
                    if (!isAssaultMove)
                    {
                        continue;
                    }
                }

                if (_teamPool.GetComponent(ally.Id).Value != observerTeam || _hitPointsPool.GetComponent(ally.Id).Current <= 0)
                {
                    continue;
                }

                var distance = (_transformPool.GetComponent(ally.Id).Value.position - observerPosition).sqrMagnitude;
                if (ally.Id == observer || distance <= assistDistance)
                {
                    _commandPool.SetComponent(ally.Id, new CommandRequest { Type = CommandType.ATTACK_TARGET, Status = CommandStatus.IDLE, Args = target });
                }
            }
        }
    }
}
