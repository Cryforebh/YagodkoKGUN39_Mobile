using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameMenuPresenter : MonoBehaviour
{
    [Inject] private IBonusModel _bonusModel;
    [Inject] private IBonusStorage _bonusStorage;

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
        SaveBonuses();

        await _sceneLoader.RestartGame();
    }

    private async UniTaskVoid MainMenuAsync()
    {
        SaveBonuses();

        await _sceneLoader.LoadMain();
    }

    private void SaveBonuses()
    {
        _bonusStorage.Save(_bonusModel);
    }
}
