using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    public async UniTask LoadMain()
    {
        await SceneManager.LoadSceneAsync("Main");
    }

    public async UniTask LoadGame()
    {
        await SceneManager.LoadSceneAsync("Game");
    }

    public async UniTask RestartGame()
    {
        await SceneManager.LoadSceneAsync("Game");
    }
}
