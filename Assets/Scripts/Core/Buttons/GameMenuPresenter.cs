using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameMenuPresenter : MonoBehaviour
{
    private ISceneLoader _sceneLoader;

    [Inject]
    private void Construct(ISceneLoader sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    public void Restart()
    {
        RestartAsync().Forget();
    }

    public void MainMenu()
    {
        MainMenuAsync().Forget();
    }

    private async UniTaskVoid RestartAsync()
    {
        await _sceneLoader.RestartGame();
    }

    private async UniTaskVoid MainMenuAsync()
    {
        await _sceneLoader.LoadMain();
    }
}
