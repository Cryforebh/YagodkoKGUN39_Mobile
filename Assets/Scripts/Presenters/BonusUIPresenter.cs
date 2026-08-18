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
        _bonusModel.CollectedBonuses
            .ObserveAdd()
            .Subscribe(_ => UpdateUI())
            .AddTo(_disposables);

        _bonusModel.CollectedBonuses
            .ObserveRemove()
            .Subscribe(_ => UpdateUI())
            .AddTo(_disposables);

        UpdateUI();
    }

    private void UpdateUI()
    {
        int stars = 0;
        int hearths = 0;

        foreach (Bonuses bonus
                 in _bonusModel.CollectedBonuses)
        {
            switch (bonus)
            {
                case Bonuses.Star:
                    stars++;
                    break;

                case Bonuses.Hearth:
                    hearths++;
                    break;
            }
        }

        _starText.text = stars.ToString();
        _hearthText.text = hearths.ToString();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
