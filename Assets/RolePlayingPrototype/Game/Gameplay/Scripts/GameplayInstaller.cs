using Game.GameEngine.Ecs;
using SampleProject.Base;
using Zenject;
using UnityEngine.Serialization;

namespace SampleProject
{
    public sealed class GameplayInstaller : MonoInstaller
    {
        [UnityEngine.SerializeField]
        [FormerlySerializedAs("unitSpawnSettings")]
        private UnitSpawnSettings _unitSpawnSettings = new();

        public override void InstallBindings()
        {
            Container.Bind<GameplayHudView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IResourceDepot>().To<CommandCenterEntity>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<EntityCommandService>().AsSingle();
            Container.BindInterfacesAndSelfTo<NavigationPathService>().AsSingle();
            Container.BindInterfacesAndSelfTo<FormationPlannerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnitSelectionService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GroupCommandService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ContextCommandResolver>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnitCollisionService>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnitSelectionPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PatrolRouteEditor>().AsSingle();
            Container.BindInterfacesAndSelfTo<PatrolHudPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ResourceStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ResourceHudPresenter>().AsSingle().NonLazy();
            Container.BindInstance(_unitSpawnSettings).AsSingle();
            Container.BindInterfacesTo<UnitGroupSpawner>().AsSingle().NonLazy();
        }
    }
}
