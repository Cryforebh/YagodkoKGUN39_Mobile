using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

public class PlayerPresenter : MonoBehaviour
{
    [SerializeField] private StickController _stickController;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private CameraMovement _cameraMovement;

    [Inject] private PlatformGenerator _platformGenerator;
    [Inject] private PlatformProgress _platformProgress;
    [Inject] private PlatformAppearance _platformAppearance;
    [Inject] private PlayerEvents _playerEvents;

    private readonly CompositeDisposable _disposables = new();

    public float PlayerOffsetY => transform.localScale.y / 14f;

    private void Awake()
    {
        _stickController.StickReady
            .Subscribe(_ => CheckLandingAsync().Forget())
            .AddTo(_disposables);
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        PlatformView currentPlatform = _platformProgress.CurrentPlatform;

        Debug.Log($"Player spawn: {currentPlatform.GetLandingPosition(PlayerOffsetY)}");
        Debug.Log($"Stick spawn: {currentPlatform.StickSpawnPosition}");

        transform.position = currentPlatform.GetLandingPosition(PlayerOffsetY);

        _stickController.ResetStick(
            currentPlatform.StickSpawnPosition);
    }

    private async UniTask CheckLandingAsync()
    {
        if (!_platformProgress.HasNextPlatform)
            return;

        Vector2 stickEnd = _stickController.GetStickEndPosition();

        PlatformView nextPlatform = _platformProgress.NextPlatform;

        bool landed =
            stickEnd.x >= nextPlatform.LeftEdge &&
            stickEnd.x <= nextPlatform.RightEdge;

        if (!landed)
        {
            Debug.Log("Неудача!");
            _playerEvents.RaisePlayerFell();
            await MovePlayerToFall();
            return;
        }

        Debug.Log("Стик попал на следующую платформу!");

        await MovePlayerToPlatform(nextPlatform);
    }

    private async UniTask MovePlayerToPlatform(PlatformView platform)
    {
        Vector3 targetPosition = platform.GetLandingPosition(PlayerOffsetY);

        await _playerMovement.MoveTo(targetPosition);

        _platformProgress.MoveToNextPlatform();

        PlatformSpawnData spawnData = _platformGenerator.PrepareNextPlatform();

        await UniTask.WhenAll(
            _platformAppearance.Show(spawnData),
            _cameraMovement.MoveToPlayer(transform));

        _platformGenerator.ReleasePreviousPlatform();

        _stickController.ResetStick(_platformProgress.CurrentPlatform.StickSpawnPosition);

        _playerEvents.RaiseReachedPlatform();

        Debug.Log("Игрок перешел на следующую платформу!");
    }

    private async UniTask MovePlayerToFall()
    {
        Vector3 stickEnd = _stickController.GetStickEndPosition();

        Vector3 fallStartPosition = new Vector3(
            stickEnd.x,
            transform.position.y,
            transform.position.z);

        await _playerMovement.MoveTo(fallStartPosition);

        await UniTask.WhenAll(
            _stickController.LowerAsync(),
            _playerMovement.FallAsync()
            );

        _playerEvents.RaisePlayerFallCompleted();
    }


    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
