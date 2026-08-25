using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class ScriptableAnimatorState : StateMachineBehaviour
    {
        [SerializeField]
        private int stateId;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.TryGetComponent(out AnimatorMachine eventDispatcher))
            {
                eventDispatcher.OnEnterState(stateInfo, this.stateId, layerIndex);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.TryGetComponent(out AnimatorMachine eventDispatcher))
            {
                eventDispatcher.OnExitState(stateInfo, this.stateId, layerIndex);
            }
        }
    }
}