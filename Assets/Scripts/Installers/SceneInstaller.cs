using UniRx;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<TimeModel>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<BonusModel>()
            .AsSingle();
        Container.Bind<PlatformRepository>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.Bind<PlatformProgress>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.Bind<PlayerEvents>()
            .AsSingle();
        Container.Bind<BonusFactory>()
            .AsSingle();
    }
}
