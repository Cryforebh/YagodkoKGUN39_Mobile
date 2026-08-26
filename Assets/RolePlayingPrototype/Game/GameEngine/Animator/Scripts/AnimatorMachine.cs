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

        private static readonly int _sTATE_PARAMETER = Animator.StringToHash("State");

        public event StateDelegate OnStateEntered;
        public event StateDelegate OnStateExited;
        public event Action<AnimationClip> OnAnimationStarted;
        public event Action<AnimationClip> OnAnimationEnded;
        
        public event Action<string> OnMessageReceived;

        public bool IsRootMotion
        {
            get { return _animator != null && _animator.applyRootMotion; }
        }

        public float BaseSpeed
        {
            get { return _baseSpeed; }
        }

        public int CurrentState
        {
            get { return _stateId; }
        }

        private int _stateId;

        private float _baseSpeed;
        
        [Space]
        [SerializeField]
[UnityEngine.Serialization.FormerlySerializedAs("animator")]         private Animator _animator;

        private readonly List<ISpeedMultiplier> _speedMultipliers = new();

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
            _stateId = _animator.GetInteger(_sTATE_PARAMETER);
            _baseSpeed = _animator.speed;
        }

        public void PlayAnimation(string animationName, string layerName, float normalizedTime = 0)
        {
            var id = Animator.StringToHash(animationName);
            this.PlayAnimation(id, layerName, normalizedTime);
        }

        public void PlayAnimation(int hash, string layerName, float normalizedTime = 0)
        {
            var index = _animator.GetLayerIndex(layerName);
            this.PlayAnimation(hash, index, normalizedTime);
        }

        public void SetLayerWeight(int layer, float weight)
        {
            _animator.SetLayerWeight(layer, weight);
        }

        public void PlayAnimation(int hash, int layer, float normalizedTime = 0)
        {
            _animator.Play(hash, layer, normalizedTime);
        }

        public void ChangeState(int stateId)
        {
            if (_stateId == stateId)
            {
                return;
            }

            _stateId = stateId;
            _animator.SetInteger(_sTATE_PARAMETER, _stateId);
        }

        public void AddSpeedMultiplier(ISpeedMultiplier multiplier)
        {
            _speedMultipliers.Add(multiplier);
            this.UpdateAnimatorSpeed();
        }

        public void RemoveSpeedMultiplier(ISpeedMultiplier multiplier)
        {
            _speedMultipliers.Remove(multiplier);
            this.UpdateAnimatorSpeed();
        }

        public void SetBaseSpeed(float speed)
        {
            if (Mathf.Approximately(speed, _baseSpeed))
            {
                return;
            }

            _baseSpeed = speed;
            this.UpdateAnimatorSpeed();
        }

        public void ApplyRootMotion()
        {
            _animator.applyRootMotion = true;
        }

        public void ResetRootMotion(bool resetPosition = true, bool resetRotation = true)
        {
            _animator.applyRootMotion = false;
            if (resetPosition)
            {
                _animator.transform.localPosition = Vector3.zero;
            }

            if (resetRotation)
            {
                _animator.transform.localRotation = Quaternion.identity;
            }
        }

        private void UpdateAnimatorSpeed()
        {
            var fullMultiplier = 1.0f;
            for (int i = 0, count = _speedMultipliers.Count; i < count; i++)
            {
                fullMultiplier *= _speedMultipliers[i].GetValue();
            }

            _animator.speed = _baseSpeed * fullMultiplier;
        }
        
        public interface ISpeedMultiplier
        {
            float GetValue();
        }
    }
}
