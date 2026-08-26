using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [RequireComponent(typeof(Entity))]
    public sealed class EntityAnimatorListener : MonoBehaviour
    {
        private Entity _entity;
        private AnimatorMachine _animator;

        private void Awake()
        {
            _entity = this.GetComponent<Entity>();
            _animator = this.GetComponentInChildren<AnimatorMachine>();
        }

        private void OnEnable()
        {
            _animator.OnMessageReceived += this.OnMessageReceived;
        }

        private void OnDisable()
        {
            _animator.OnMessageReceived -= this.OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            _entity.SendEvent(new AnimatorEvent {Message = message});
        }
    }
}