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
            if (hasCommand && !canRetarget && !isPatrolCommand && !isPatrolCombat)
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
                    ResumePatrol(entity);
                }

                return;
            }

            if (isPatrolCombat || canRetarget)
            {
                RetargetIfCloser(entity, target, vision.Range);
                return;
            }

            if (isPatrolCommand)
            {
                StartPatrolCombat(entity, target);
            }

            AlertNearbyAllies(entity, target, vision.AssistRange);
        }

        private void StartPatrolCombat(int entity, EntityHandle target)
        {
            _commandPool.SetComponent(entity, new CommandRequest
            {
                Type = CommandType.ATTACK_TARGET,
                Status = CommandStatus.IDLE,
                Args = target
            });
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
            var observerTeam = _teamPool.GetComponent(observer).Value;
            var observerPosition = _transformPool.GetComponent(observer).Value.position;
            var bestDistance = range * range;
            var result = EntityHandle.Invalid;
            _world.GetActiveEntities(_entities);

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
                if (!EcsFilter.Matches(ally.Id, _teamPool, _transformPool, _hitPointsPool) || _commandPool.HasComponent(ally.Id))
                {
                    continue;
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
