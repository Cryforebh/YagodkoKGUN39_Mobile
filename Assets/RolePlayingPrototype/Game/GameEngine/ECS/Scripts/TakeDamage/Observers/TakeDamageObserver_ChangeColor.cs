using System.Collections;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class TakeDamageObserver_ChangeColor : IEcsObserver<TakeDamageEvent>
    {
        private readonly EcsPool<RendererComponent> _meshPool;

        void IEcsObserver<TakeDamageEvent>.Handle(int entity, TakeDamageEvent takeDamageEvent)
        {
            if (!_meshPool.HasComponent(entity))
            {
                return;
            }

            ref var meshComponent = ref _meshPool.GetComponent(entity);
            meshComponent.Value.GetComponentInParent<Entity>().StartCoroutine(this.Red(meshComponent));
        }

        private IEnumerator Red(RendererComponent rendererComponent)
        {
            rendererComponent.Value.material.SetColor("_BaseColor", Color.red);
            yield return new WaitForSeconds(0.25f);
            rendererComponent.Value.material.SetColor("_BaseColor", Color.white);
        }
    }
}