using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    private const string MainSceneName = "Main";
    private const string GameSceneName = "Game";

    public async UniTask LoadMain()
    {
        await SceneManager.LoadSceneAsync(MainSceneName);
    }

    public async UniTask LoadGame()
    {
        await SceneManager.LoadSceneAsync(GameSceneName);
    }
}
