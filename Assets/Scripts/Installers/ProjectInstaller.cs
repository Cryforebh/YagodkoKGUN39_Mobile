using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        //Container.Bind<ISceneLoader>()
        //    .To<SceneLoader>()
        //    .AsSingle();
        Container.Bind<ISceneLoader>().To<SceneLoader>().FromInstance(_sceneLoader).AsSingle();
    }
}
