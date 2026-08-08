using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class BonusUIPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _starText;
    [SerializeField] private TMP_Text _hearthText;

    [Inject] private IBonusModel _bonusModel;

    private readonly CompositeDisposable _disposables = new();

    private int _starCount;
    private int _hearthCount;

    private void Start()
    {
        UpdateUI();

        _bonusModel.CollectedBonuses
            .ObserveAdd()
            .Subscribe(OnBonusAdded)
            .AddTo(_disposables);

        _bonusModel.CollectedBonuses
            .ObserveRemove()
            .Subscribe(OnBonusRemoved)
            .AddTo(_disposables);
    }

    private void OnBonusAdded(CollectionAddEvent<Bonuses> eventData)
    {
        switch (eventData.Value)
        {
            case Bonuses.Star:
                _starCount++;
                break;

            case Bonuses.Hearth:
                _hearthCount++;
                break;
        }

        UpdateUI();
    }

    private void OnBonusRemoved(CollectionRemoveEvent<Bonuses> eventData)
    {
        switch (eventData.Value)
        {
            case Bonuses.Star:
                _starCount--;
                break;

            case Bonuses.Hearth:
                _hearthCount--;
                break;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        _starText.text = $"{_starCount}";
        _hearthText.text = $"{_hearthCount}";
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
