using UnityEngine;
using Zenject;

public class PlatformGenerator : MonoBehaviour, IInitializable
{
    [Header("Стартовая Платформа:")]
    [SerializeField] private PlatformView _firstPlatformPrefab;
    [SerializeField] private Vector3 _firstPlatformPosition;

    [Header("Генерация:")]
    [SerializeField] private float _platformY = -4.77f;
    [SerializeField, Min(0f)] private float _minGap = 2f;
    [SerializeField, Min(0f)] private float _maxGap = 5f;

    [Header("Иерархия")]
    [SerializeField] private Transform _activePlatformsRoot;

    [Inject] private PlatformPool _platformPool;
    [Inject] private PlatformProgress _platformProgress;

    public void Initialize()
    {
        CreateInitialPlatforms();
    }

    private void CreateInitialPlatforms()
    {
        PlatformView firstPlatform = _platformPool.Take(_firstPlatformPrefab);

        if (firstPlatform == null)
            return;

        firstPlatform.transform.SetParent(_activePlatformsRoot);
        firstPlatform.Activate(_firstPlatformPosition);

        PlatformView secondPlatform = _platformPool.TakeRandom();

        if (secondPlatform == null)
        {
            _platformPool.Return(firstPlatform);
            return;
        }

        PlaceAfter(secondPlatform, firstPlatform);

        _platformProgress.Initialize(firstPlatform, secondPlatform);
    }

    public PlatformSpawnData PrepareNextPlatform()
    {
        PlatformView currentPlatform = _platformProgress.CurrentPlatform;

        if (currentPlatform == null)
            return default;

        if (_platformProgress.NextPlatform != null)
            return default;

        PlatformView nextPlatform = _platformPool.TakeRandom();

        if (nextPlatform == null)
            return default;

        nextPlatform.transform.SetParent(_activePlatformsRoot);

        Vector3 targetPosition = CalculatePositionAfter(nextPlatform, currentPlatform);

        //PlaceAfter(nextPlatform, _platformProgress.CurrentPlatform);

        _platformProgress.SetNextPlatform(nextPlatform);

        return new PlatformSpawnData(nextPlatform, targetPosition);
    }

    public void ReleasePreviousPlatform()
    {
        PlatformView previousPlatform = _platformProgress.PreviousPlatform;

        if (previousPlatform == null)
            return;

        _platformPool.Return(previousPlatform);
        _platformProgress.ClearPreviousPlatform();
    }

    private void PlaceAfter(PlatformView platform, PlatformView previousPlatform)
    {
        platform.transform.SetParent(_activePlatformsRoot);

        Vector3 targetPosition =
            CalculatePositionAfter(
                platform,
                previousPlatform);

        platform.Activate(targetPosition);
    }

    private Vector3 CalculatePositionAfter(PlatformView platform, PlatformView previousPlatform)
    {
        float gap = Random.Range(_minGap, _maxGap);

        float targetX = previousPlatform.RightEdge + gap + platform.PivotToLeftEdge;

        return new Vector3(
            targetX,
            _platformY,
            _firstPlatformPosition.z);
    }
}