using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class BonusCollectorPresenter : MonoBehaviour
{
    [Inject] private IBonusModel _bonusModel;
    [Inject] private IMessageBroker _messageBroker;

    private readonly CompositeDisposable _disposables = new();

    private void Awake()
    {
        _messageBroker
            .Receive<BonusSpawnedMessage>()
            .Subscribe(OnBonusSpawned)
            .AddTo(_disposables);
    }

    private void OnBonusSpawned(BonusSpawnedMessage message)
    {
        BonusView bonus = message.Bonus;

        if (bonus == null)
            return;

        bonus.Collected += OnBonusCollected;
    }

    private void OnBonusCollected(BonusView bonus)
    {
        _bonusModel.Add(bonus.BonusType);

        bonus.Collected -= OnBonusCollected;

        Destroy(bonus.gameObject);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
