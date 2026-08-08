using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class BonusGeneratorPresenter : MonoBehaviour
{
    [Inject] private PlayerEvents _playerEvents;

    private readonly CompositeDisposable _disposables = new();

    private void Awake()
    {
        _playerEvents.ReachedPlatform
            .Subscribe(_ => GenerateBonuses())
            .AddTo(_disposables);
    }

    private void GenerateBonuses()
    {
        Debug.Log("Генерируем бонусы!");
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
