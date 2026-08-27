using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_AttackTarget : CommandState
    {
        private EcsPool<AttackTarget> _attackPool;

        private EcsPool<HitRequest> _hitRequestPool;
        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<HitPointsComponent> _hitPointsPool;

        private EcsWorld _world;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.ATTACK_TARGET;
        }

        public override void Enter(int entity, object args)
        {
            _attackPool.SetComponent(entity, new AttackTarget
            {
                Target = (EntityHandle) args
            });
        }

        public override void Exit(int entity)
        {
            _attackPool.RemoveComponent(entity);
            _hitRequestPool.RemoveComponent(entity);
            _moveToPositionPool.RemoveComponent(entity);
        }

        public override void Update(int entity)
        {
            if (!this.IsTargetExists(entity))
            {
                this.Complete(entity);
            }
        }

        private bool IsTargetExists(int entity)
        {
            if (!_attackPool.HasComponent(entity))
            {
                return false;
            }

            ref var target = ref _attackPool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_hitPointsPool.HasComponent(target.Id))
            {
                return false;
            }
            
            ref var targetHitPoints = ref _hitPointsPool.GetComponent(target.Id);
            return targetHitPoints.Current > 0;
        }
    }
}
