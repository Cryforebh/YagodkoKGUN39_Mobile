using Game.GameEngine.Ecs;
using GameECS;

namespace SampleProject
{
    public sealed class CharacterAnimatorSystem : IEcsUpdate
    {
        private EcsPool<AnimatorComponent> animatorPool;

        private EcsPool<MoveStepData> moveStep;
        private EcsPool<HitDuration> attackPool;
        private EcsPool<GatherDuration> gatherPool;

        void IEcsUpdate.Update(int entity)
        {
            ref var animator = ref this.animatorPool.GetComponent(entity).value;
            var animatorState = this.ResolveState(entity);
            animator.ChangeState(animatorState);
        }

        private int ResolveState(int entity)
        {
            if (this.attackPool.HasComponent(entity))
            {
                return AnimatorStateId.ATTACK;
            }

            if (this.gatherPool.HasComponent(entity))
            {
                return AnimatorStateId.GATHERING;
            }

            if (this.moveStep.HasComponent(entity))
            {
                return AnimatorStateId.MOVE;
            }

            return AnimatorStateId.IDLE;
        }
    }
}