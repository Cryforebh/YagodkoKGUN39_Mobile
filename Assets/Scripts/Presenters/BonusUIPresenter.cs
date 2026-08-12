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

    [Inject] private IBonusStorage _bonusStorage;
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

        //_bonusModel.CollectedBonuses
        //    .ObserveReset()
        //    .Subscribe(_ => OnBonusesReset())
        //    .AddTo(_disposables);
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

    //private void OnBonusesReset()
    //{
    //    _starCount = 0;
    //    _hearthCount = 0;

    //    UpdateUI();
    //}

    private void UpdateUI()
    {
        int savedStars = _bonusStorage.StarCount;
        int savedHearths = _bonusStorage.HearthCount;

        _starText.text = $"{savedStars + _starCount}";
        _hearthText.text = $"{savedHearths + _hearthCount}";
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
