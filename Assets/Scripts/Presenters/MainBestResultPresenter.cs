using TMPro;
using UnityEngine;
using Zenject;

public class MainBestResultPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _platformText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _starText;
    [SerializeField] private TMP_Text _hearthText;

    [Inject] private IPlatformScoreStorage _scoreStorage;
    [Inject] private IBonusStorage _bonusStorage;

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        _platformText.text = $"P: {_scoreStorage.BestScore}";

        if (_timeText)
            _timeText.text = $"T: {_scoreStorage.BestTime} sec.";

        _starText.text = _bonusStorage.BestStarCount.ToString();

        _hearthText.text = _bonusStorage.BestHearthCount.ToString();
    }
}
