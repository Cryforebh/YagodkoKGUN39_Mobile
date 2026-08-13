using UniRx;
using UnityEngine;
using Zenject;

[CreateAssetMenu(
        fileName = "ProjectInstaller",
        menuName = "Installers/New ProjectInstaller"
    )]
public class ProjectInstaller : ScriptableObjectInstaller
{
    private SceneLoader _sceneLoader = new SceneLoader();

    public override void InstallBindings()
    {
        Container.Bind<ISceneLoader>().To<SceneLoader>()
            .FromInstance(_sceneLoader)
            .AsSingle();
        Container.BindInterfacesAndSelfTo<MessageBroker>()
            .AsSingle();
        Container.Bind<IBonusStorage>()
            .To<BonusStorage>()
            .AsSingle();
    }
}
