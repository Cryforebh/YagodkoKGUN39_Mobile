using System.Collections;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class TakeDamageObserver_ChangeColor : IEcsObserver<TakeDamageEvent>
    {
        private readonly EcsPool<RendererComponent> meshPool;

        void IEcsObserver<TakeDamageEvent>.Handle(int entity, TakeDamageEvent takeDamageEvent)
        {
            if (!this.meshPool.HasComponent(entity))
            {
                return;
            }

            ref var meshComponent = ref this.meshPool.GetComponent(entity);
            meshComponent.value.GetComponentInParent<Entity>().StartCoroutine(this.Red(meshComponent));
        }

        private IEnumerator Red(RendererComponent rendererComponent)
        {
            rendererComponent.value.material.SetColor("_BaseColor", Color.red);
            yield return new WaitForSeconds(0.25f);
            rendererComponent.value.material.SetColor("_BaseColor", Color.white);
        }
    }
}