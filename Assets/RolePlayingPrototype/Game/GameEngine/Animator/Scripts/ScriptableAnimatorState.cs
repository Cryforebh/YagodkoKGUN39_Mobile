using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class ScriptableAnimatorState : StateMachineBehaviour
    {
        [SerializeField]
[UnityEngine.Serialization.FormerlySerializedAs("stateId")]         private int _stateId;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.TryGetComponent(out AnimatorMachine eventDispatcher))
            {
                eventDispatcher.OnEnterState(stateInfo, _stateId, layerIndex);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.TryGetComponent(out AnimatorMachine eventDispatcher))
            {
                eventDispatcher.OnExitState(stateInfo, _stateId, layerIndex);
            }
        }
    }
}