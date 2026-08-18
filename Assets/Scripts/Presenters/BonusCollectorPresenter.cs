using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class BonusCollectorPresenter : MonoBehaviour
{
    [Inject] private PlayerEvents _playerEvents;
    [Inject] private IBonusModel _bonusModel;
    [Inject] private IMessageBroker _messageBroker;
    [Inject] private IBonusStorage _bonusStorage;

    private readonly List<Bonuses> _currentAttemptBonuses = new();
    private readonly CompositeDisposable _disposables = new();

    private bool _isAttemptFailed;

    private void Awake()
    {
        _messageBroker
            .Receive<BonusSpawnedMessage>()
            .Subscribe(OnBonusSpawned)
            .AddTo(_disposables);

        _playerEvents.ReachedPlatform
            .Subscribe(_ => ConfirmCurrentAttempt())
            .AddTo(_disposables);

        _playerEvents.PlayerFell
            .Subscribe(_ => CancelCurrentAttempt())
            .AddTo(_disposables);
    }

    private void Start()
    {
        _bonusStorage.Clear();
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
        if (bonus == null)
            return;

        Bonuses bonusType = bonus.BonusType;

        bonus.Collected -= OnBonusCollected;

        Destroy(bonus.gameObject);

        if (_isAttemptFailed)
            return;

        _bonusModel.Add(bonusType);
        _currentAttemptBonuses.Add(bonusType);
    }

    private void ConfirmCurrentAttempt()
    {
        _currentAttemptBonuses.Clear();
        _isAttemptFailed = false;

        _bonusStorage.Save(_bonusModel);
    }

    private void CancelCurrentAttempt()
    {
        _isAttemptFailed = true;

        RollbackCurrentAttempt();

        _bonusStorage.Save(_bonusModel);
        _bonusStorage.Flush();
    }

    private void RollbackCurrentAttempt()
    {
        foreach (Bonuses bonusType in _currentAttemptBonuses)
            _bonusModel.Remove(bonusType);

        _currentAttemptBonuses.Clear();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            _bonusStorage.Flush();
    }

    private void OnApplicationQuit()
    {
        _bonusStorage.Flush();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
