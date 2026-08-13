using TMPro;
using UnityEngine;
using Zenject;

public class MainBonusPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _starText;
    [SerializeField] private TMP_Text _hearthText;

    [Inject] private IBonusStorage _bonusStorage;

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        int starCount = _bonusStorage.StarCount;
        int hearthCount = _bonusStorage.HearthCount;

        _starText.text = $"{starCount}";
        _hearthText.text = $"{hearthCount}";
    }
}
