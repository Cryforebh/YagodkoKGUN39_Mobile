using GameECS;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_AttackTarget : CommandState
    {
        private EcsPool<AttackTarget> attackPool;

        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<HitPointsComponent> hitPointsPool;

        private EcsWorld world;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.ATTACK_TARGET;
        }

        public override void Enter(int entity, object args)
        {
            this.attackPool.SetComponent(entity, new AttackTarget
            {
                targetId = ((Entity) args).Id
            });
        }

        public override void Exit(int entity)
        {
            this.attackPool.RemoveComponent(entity);
            this.hitRequestPool.RemoveComponent(entity);
            this.moveToPositionPool.RemoveComponent(entity);
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
            ref var targetId = ref this.attackPool.GetComponent(entity).targetId;
            if (!this.world.IsEntityExists(targetId))
            {
                return false;
            }
            
            ref var targetHitPoints = ref this.hitPointsPool.GetComponent(targetId);
            return targetHitPoints.current > 0;
        }
    }
}