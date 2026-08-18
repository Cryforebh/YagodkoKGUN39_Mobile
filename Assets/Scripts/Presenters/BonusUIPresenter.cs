using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class BonusUIPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _starText;
    [FormerlySerializedAs("_hearthText")]
    [SerializeField] private TMP_Text _heartText;

    [Inject] private IBonusModel _bonusModel;

    private readonly CompositeDisposable _disposables = new();

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
        int hearts = 0;

        foreach (BonusType bonus
                 in _bonusModel.CollectedBonuses)
        {
            switch (bonus)
            {
                case BonusType.Star:
                    stars++;
                    break;

                case BonusType.Heart:
                    hearts++;
                    break;
            }
        }

        _starText.text = stars.ToString();
        _heartText.text = hearts.ToString();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
