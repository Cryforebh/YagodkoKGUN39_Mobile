using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class GameOverPresenter : MonoBehaviour
{
    [Header("Interfaces")]
    [SerializeField] private GameObject _gameInterface;
    [SerializeField] private RectTransform _gameOverPanel;

    [Header("Result")]
    [SerializeField] private TMP_Text _timeResultText;
    [SerializeField] private TMP_Text _platformResultText;
    [SerializeField] private TMP_Text _bestPlatformResultText;
    [FormerlySerializedAs("_hearthResultText")]
    [SerializeField] private TMP_Text _heartResultText;
    [SerializeField] private TMP_Text _starResultText;

    [Header("Animation")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _hiddenOffset = 100f;

    [Inject] private ITimeModel _timeModel;
    [Inject] private PlatformProgress _platformProgress;
    [Inject] private IPlatformScoreStorage _platformScoreStorage;
    [Inject] private IBonusStorage _bonusStorage;
    [Inject] private IBonusModel _bonusModel;
    [Inject] private PlayerEvents _playerEvents;

    private readonly CompositeDisposable _disposables = new();

    private CancellationToken _cancellationToken;
    private Vector2 _shownPosition;
    private bool _isGameOver;

    private void Awake()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();

        _shownPosition = _gameOverPanel.anchoredPosition;

        _gameOverPanel.gameObject.SetActive(false);

        _playerEvents.PlayerFallStarted
            .Subscribe(_ => BeginGameOver())
            .AddTo(_disposables);

        _playerEvents.PlayerFallCompleted
            .Subscribe(_ => ShowGameOverAsync().Forget())
            .AddTo(_disposables);
    }

    private void BeginGameOver()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;

        _timeModel.StopTimer();

        int currentScore = _platformProgress.PassedPlatforms;
        int currentTime = _timeModel.Time.Value;

        _platformScoreStorage.SaveIfBest(currentScore, currentTime);
        _bonusStorage.SaveBest(_bonusModel);
        _bonusStorage.Flush();

        if (_timeResultText)
            _timeResultText.text = $"Time: {_timeModel.Time.Value} sec.";

        _starResultText.text = _bonusStorage.StarCount.ToString();
        _heartResultText.text = _bonusStorage.HeartCount.ToString();

        _platformResultText.text = $"Platforms: {_platformProgress.PassedPlatforms}";

        _bestPlatformResultText.text = $"Best Score: {_platformScoreStorage.BestScore}";

        _gameInterface.SetActive(false);
    }

    private async UniTask ShowGameOverAsync()
    {
        if (!_isGameOver)
            return;

        Vector2 hiddenPosition =
            _shownPosition +
            Vector2.up * (_gameOverPanel.rect.height + _hiddenOffset);

        _gameOverPanel.anchoredPosition = hiddenPosition;

        _gameOverPanel.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / _animationDuration);

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            _gameOverPanel.anchoredPosition = Vector2.Lerp(hiddenPosition, _shownPosition, smoothProgress);

            await UniTask.Yield(
                PlayerLoopTiming.Update,
                _cancellationToken);
        }

        _gameOverPanel.anchoredPosition = _shownPosition;
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
