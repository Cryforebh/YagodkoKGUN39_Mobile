using GameECS;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class EcsModule : MonoInstaller
    {
        [SerializeField]
[UnityEngine.Serialization.FormerlySerializedAs("installers")]         private EcsInstaller[] _installers;

        public override void InstallBindings()
        {
            var eventBus = new EcsReactiveBus();
            var world = new EcsWorld(eventBus);
            world.SetExternalInjector(Container.Inject);
            
            foreach (var installer in _installers)
            {
                installer.Install(world);
            }

            Container.Bind<EcsWorld>().FromInstance(world).AsSingle();
            Container.BindInterfacesAndSelfTo<EcsReactiveBus>().FromInstance(eventBus).AsSingle();
            Container.BindInterfacesAndSelfTo<EcsLoop>().AsSingle().NonLazy();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Container != null && Container.HasBinding<EcsWorld>())
            {
                Container.Resolve<EcsWorld>().OnDrawGizmos();
            }
        }
#endif
    }
}
