using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    private const string MainSceneName = "Main";
    private const string GameSceneName = "Game";

    private readonly SceneTransitionView _transitionView;

    private bool _isLoading;

    public SceneLoader(SceneTransitionView transitionView)
    {
        _transitionView = transitionView;
    }

    public UniTask LoadMain()
    {
        return LoadSceneAsync(MainSceneName);
    }

    public UniTask LoadGame()
    {
        return LoadSceneAsync(GameSceneName);
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            // Начинаем загрузку сразу после нажатия
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            if (operation == null)
            {
                throw new InvalidOperationException($"Не удалось загрузить сцену {sceneName}.");
            }

            // Не позволяем новой сцене активироваться, пока экран не закрылся.
            operation.allowSceneActivation = false;

            // Пока работает шейдер, сцена уже загружается.
            await _transitionView.CloseAsync();

            // Если после закрытия загрузка еще не готова, показываем надпись.
            if (operation.progress < 0.9f)
                _transitionView.ShowLoading(true);

            while (operation.progress < 0.9f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // Отрисовываем полостью черный кадр перед активацией сцены.
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            operation.allowSceneActivation = true;

            await operation;

            _transitionView.ShowLoading(false);

            await _transitionView.OpenAsync();
        }
        catch
        {
            _transitionView.ShowLoading(false);

            await _transitionView.OpenAsync();

            throw;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
