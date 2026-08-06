using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MainMenuPresenter : MonoBehaviour
{
    private ISceneLoader _sceneLoader;

    [Inject]
    private void Construct(ISceneLoader sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    public void Play()
    {
        Load().Forget();
    }

    private async UniTaskVoid Load()
    {
        Debug.LogError(_sceneLoader != null);
        await _sceneLoader.LoadGame();
    }
}
