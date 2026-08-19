using UniRx;
using UnityEngine;
using Zenject;

[CreateAssetMenu(
        fileName = "ProjectInstaller",
        menuName = "Installers/New ProjectInstaller"
    )]
public class ProjectInstaller : ScriptableObjectInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ISceneLoader>()
            .To<SceneLoader>()
            .AsSingle();
        Container.Bind<SceneTransitionView>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<MessageBroker>()
            .AsSingle();
        Container.Bind<IBonusStorage>()
            .To<BonusStorage>()
            .AsSingle();
        Container.Bind<IPlatformScoreStorage>()
            .To<PlatformScoreStorage>()
            .AsSingle();

        Container.Bind<SfxPlayer>()
            .FromComponentInHierarchy()
            .AsSingle();
    }
}
