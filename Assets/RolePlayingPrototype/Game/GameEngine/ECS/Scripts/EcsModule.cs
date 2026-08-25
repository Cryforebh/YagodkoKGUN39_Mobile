using GameECS;
using SampleProject;
using SampleProject.Base;
using UnityEngine;
using Zenject;

namespace Game.GameEngine.Ecs
{
    public sealed class EcsModule : MonoInstaller
    {
        [SerializeField]
        private EcsInstaller[] installers;

        public override void InstallBindings()
        {
            Container.Bind<CommandCenterEntity>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<EntityCommandService>().AsSingle();

            var eventBus = new EcsReactiveBus();
            var world = new EcsWorld(eventBus);
            world.SetExternalInjector(Container.Inject);
            
            foreach (var installer in installers)
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
