using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionView : MonoBehaviour
{
    private static readonly int RadiusHash = Shader.PropertyToID("_Radius");

    [SerializeField] private GameObject _overlayRoot;
    [SerializeField] private Image _irisImage;
    [SerializeField] private TMP_Text _loadingText;

    [SerializeField, Min(0.45f)] private float _animationDuration = 0.45f;

    private Material _material;
    private CancellationToken _cancellationToken;

    private void Awake()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();

        _material = Instantiate(_irisImage.material);

        _irisImage.material = _material;

        SetRadius(1f);
        ShowLoading(false);

        _overlayRoot.SetActive(false);
    }

    public async UniTask CloseAsync()
    {
        _overlayRoot.SetActive(true);
        ShowLoading(false);

        await AnimateRadiusAsync(from: 1f, to: 0f);

        SetRadius(0f);
    }

    public async UniTask OpenAsync()
    {
        ShowLoading(false);
        SetRadius(0f);

        // Отрисовка первого кадра в новой сцене под закрытым экраном
        await UniTask.Yield(
            PlayerLoopTiming.LastPostLateUpdate,
            _cancellationToken);

        await AnimateRadiusAsync(from: 0f, to: 1f);

        SetRadius(1f);
        _overlayRoot.SetActive(false);
    }

    public void ShowLoading(bool show)
    {
        _loadingText.gameObject.SetActive(show);
    }

    private async UniTask AnimateRadiusAsync(float from, float to)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            float frameDelta = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);

            elapsedTime += frameDelta;

            float progress = Mathf.Clamp01(elapsedTime / _animationDuration);

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            SetRadius(Mathf.Lerp(
                    from,
                    to,
                    smoothProgress));

            await UniTask.Yield(
                PlayerLoopTiming.LastPostLateUpdate,
                _cancellationToken);
        }

        SetRadius(to);
    }

    private void SetRadius(float radius)
    {
        _material.SetFloat(RadiusHash, radius);
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}
