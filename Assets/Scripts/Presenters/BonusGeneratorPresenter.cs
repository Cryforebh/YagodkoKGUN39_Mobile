using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class BonusGeneratorPresenter : MonoBehaviour
{
    [SerializeField] private BonusView _starPrefab;
    [SerializeField] private BonusView _hearthPrefab;

    [SerializeField, Range(0f, 1f)] private float _spawnChance = 0.7f;
    [SerializeField] private int _bonusCount = 3;
    [SerializeField] private float _bonusHeight = 0.35f;
    [SerializeField] private float _bonusEdgeOffset = 0.5f;
    [SerializeField] private float _minGapWidth = 2f;

    [Inject] private PlayerEvents _playerEvents;
    [Inject] private PlatformProgress _platformProgress;
    [Inject] private BonusFactory _bonusFactory;

    private readonly List<BonusView> _spawnedBonuses = new();

    private readonly CompositeDisposable _disposables = new();

    private void Awake()
    {
        _playerEvents.ReachedPlatform
            .Subscribe(_ => GenerateBonuses())
            .AddTo(_disposables);

        _playerEvents.PlayerFallCompleted
            .Subscribe(_ => ClearPreviousBonuses())
            .AddTo(_disposables);
    }

    private void GenerateBonuses()
    {
        ClearPreviousBonuses();

        if (Random.value > _spawnChance)
            return;

        if (!_platformProgress.HasNextPlatform)
            return;

        PlatformView currentPlatform = _platformProgress.CurrentPlatform;
        PlatformView nextPlatform = _platformProgress.NextPlatform;

        float left = currentPlatform.RightEdge;
        float right = nextPlatform.LeftEdge;

        float width = right - left;

        if (width < _minGapWidth)
            return;

        for (int i = 0; i < _bonusCount; i++)
        {
            float x = Random.Range(
                left + _bonusEdgeOffset,
                right - _bonusEdgeOffset);

            float y = Mathf.Max(
                currentPlatform.TopEdge,
                nextPlatform.TopEdge) + _bonusHeight;

            Vector3 spawnPosition = new Vector3(
                x,
                y,
                0f);

            BonusView prefab = Random.value < 0.5f
                ? _starPrefab
                : _hearthPrefab;

            BonusView bonus = _bonusFactory.Create(prefab, spawnPosition);

            _spawnedBonuses.Add(bonus);
        }
    }

    private void ClearPreviousBonuses()
    {
        foreach (BonusView bonus in _spawnedBonuses)
        {
            if (bonus != null)
                Destroy(bonus.gameObject);
        }
        _spawnedBonuses.Clear();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}