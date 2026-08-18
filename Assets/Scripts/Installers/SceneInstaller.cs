using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<TimeModel>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<BonusModel>()
            .AsSingle();
        Container.Bind<PlatformProgress>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.Bind<PlayerEvents>()
            .AsSingle();
        Container.Bind<BonusFactory>()
            .AsSingle();
        Container.Bind<PlatformPool>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<PlatformGenerator>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.Bind<PlatformAppearance>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.Bind<BonusPool>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
