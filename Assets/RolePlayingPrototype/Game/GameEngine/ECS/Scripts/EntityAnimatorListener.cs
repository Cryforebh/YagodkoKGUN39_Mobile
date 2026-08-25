using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [RequireComponent(typeof(Entity))]
    public sealed class EntityAnimatorListener : MonoBehaviour
    {
        private Entity entity;
        private AnimatorMachine animator;

        private void Awake()
        {
            this.entity = this.GetComponent<Entity>();
            this.animator = this.GetComponentInChildren<AnimatorMachine>();
        }

        private void OnEnable()
        {
            this.animator.OnMessageReceived += this.OnMessageReceived;
        }

        private void OnDisable()
        {
            this.animator.OnMessageReceived -= this.OnMessageReceived;
        }

        private void OnMessageReceived(string message)
        {
            this.entity.SendEvent(new AnimatorEvent {message = message});
        }
    }
}