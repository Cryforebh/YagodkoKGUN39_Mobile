using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.GameEngine.Ecs
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorMachine : MonoBehaviour
    {
        public delegate void StateDelegate(AnimatorStateInfo state, int stateId, int layerIndex);

        private static readonly int STATE_PARAMETER = Animator.StringToHash("State");

        public event StateDelegate OnStateEntered;
        public event StateDelegate OnStateExited;
        public event Action<AnimationClip> OnAnimationStarted;
        public event Action<AnimationClip> OnAnimationEnded;
        
        public event Action<string> OnMessageReceived;

        public bool IsRootMotion
        {
            get { return this.animator != null && this.animator.applyRootMotion; }
        }

        public float BaseSpeed
        {
            get { return this.baseSpeed; }
        }

        public int CurrentState
        {
            get { return this.stateId; }
        }

        private int stateId;

        private float baseSpeed;
        
        [Space]
        [SerializeField]
        private Animator animator;

        private readonly List<ISpeedMultiplier> speedMultipliers = new();

        public void OnEnterState(AnimatorStateInfo state, int stateId, int layerIndex)
        {
            this.OnStateEntered?.Invoke(state, stateId, layerIndex);
        }
        
        public void OnExitState(AnimatorStateInfo state, int stateId, int layerIndex)
        {
            this.OnStateExited?.Invoke(state, stateId, layerIndex);
        }

        public void ReceiveStartAnimation(AnimationClip clip)
        {
            this.OnAnimationStarted?.Invoke(clip);
        }

        public void ReceiveEndAnimation(AnimationClip clip)
        {
            this.OnAnimationEnded?.Invoke(clip);
        }
        
        public void ReceiveString(string message) 
        {
            this.OnMessageReceived?.Invoke(message);
        }

        protected virtual void Awake()
        {
            this.stateId = this.animator.GetInteger(STATE_PARAMETER);
            this.baseSpeed = this.animator.speed;
        }

        public void PlayAnimation(string animationName, string layerName, float normalizedTime = 0)
        {
            var id = Animator.StringToHash(animationName);
            this.PlayAnimation(id, layerName, normalizedTime);
        }

        public void PlayAnimation(int hash, string layerName, float normalizedTime = 0)
        {
            var index = this.animator.GetLayerIndex(layerName);
            this.PlayAnimation(hash, index, normalizedTime);
        }

        public void SetLayerWeight(int layer, float weight)
        {
            this.animator.SetLayerWeight(layer, weight);
        }

        public void PlayAnimation(int hash, int layer, float normalizedTime = 0)
        {
            this.animator.Play(hash, layer, normalizedTime);
        }

        public void ChangeState(int stateId)
        {
            if (this.stateId == stateId)
            {
                return;
            }

            this.stateId = stateId;
            this.animator.SetInteger(STATE_PARAMETER, this.stateId);
        }

        public void AddSpeedMultiplier(ISpeedMultiplier multiplier)
        {
            this.speedMultipliers.Add(multiplier);
            this.UpdateAnimatorSpeed();
        }

        public void RemoveSpeedMultiplier(ISpeedMultiplier multiplier)
        {
            this.speedMultipliers.Remove(multiplier);
            this.UpdateAnimatorSpeed();
        }

        public void SetBaseSpeed(float speed)
        {
            if (Mathf.Approximately(speed, this.baseSpeed))
            {
                return;
            }

            this.baseSpeed = speed;
            this.UpdateAnimatorSpeed();
        }

        public void ApplyRootMotion()
        {
            this.animator.applyRootMotion = true;
        }

        public void ResetRootMotion(bool resetPosition = true, bool resetRotation = true)
        {
            this.animator.applyRootMotion = false;
            if (resetPosition)
            {
                this.animator.transform.localPosition = Vector3.zero;
            }

            if (resetRotation)
            {
                this.animator.transform.localRotation = Quaternion.identity;
            }
        }

        private void UpdateAnimatorSpeed()
        {
            var fullMultiplier = 1.0f;
            for (int i = 0, count = this.speedMultipliers.Count; i < count; i++)
            {
                fullMultiplier *= this.speedMultipliers[i].GetValue();
            }

            this.animator.speed = this.baseSpeed * fullMultiplier;
        }
        
        public interface ISpeedMultiplier
        {
            float GetValue();
        }
    }
}
