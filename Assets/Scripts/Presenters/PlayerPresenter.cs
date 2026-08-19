using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

public class PlayerPresenter : MonoBehaviour
{
    [SerializeField] private StickController _stickController;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private CameraMovement _cameraMovement;
    [SerializeField] private PlayerAnimation _playerAnimation;
    [SerializeField] private RobotAudio _robotAudio;

    [Inject] private PlatformGenerator _platformGenerator;
    [Inject] private PlatformProgress _platformProgress;
    [Inject] private PlatformAppearance _platformAppearance;
    [Inject] private PlayerEvents _playerEvents;

    private readonly CompositeDisposable _disposables = new();

    public float PlayerOffsetY => transform.localScale.y / 9f;

    private void Awake()
    {
        _stickController.StickReady
            .Subscribe(_ => CheckLandingAsync().Forget())
            .AddTo(_disposables);

        _stickController.GrowthStarted
            .Subscribe(_ =>
            {
                _playerAnimation.PlayFire();
                _robotAudio.StartCreateStick();
            })
            .AddTo(_disposables);

        _stickController.GrowthFinished
            .Subscribe(_ =>
            {
                _robotAudio.StopCreateStick();
                PlayPushAndDropStickAsync().Forget();
            })
            .AddTo(_disposables);
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        PlatformView currentPlatform = _platformProgress.CurrentPlatform;

        transform.position = currentPlatform.GetLandingPosition(PlayerOffsetY);

        _stickController.ResetStick(
            currentPlatform.StickSpawnPosition);

        _playerAnimation.PlayIdle();
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
            _playerEvents.RaisePlayerFell();
            await MovePlayerToFall();
            return;
        }

        await MovePlayerToPlatform(nextPlatform);
    }

    private async UniTask PlayPushAndDropStickAsync()
    {
        await _playerAnimation.PlayPushAsync();

        _stickController.StartRotation();
    }

    private async UniTask MovePlayerToPlatform(PlatformView platform)
    {
        Vector3 targetPosition = platform.GetLandingPosition(PlayerOffsetY);

        _playerAnimation.PlayRun();
        _robotAudio.StartMotor();

        await _playerMovement.MoveTo(targetPosition);

        _playerAnimation.PlayIdle();
        _robotAudio.StopMotor();

        _platformProgress.MoveToNextPlatform();

        float cameraRightEdge = _cameraMovement.GetRightEdgeAfterMove(transform);

        PlatformSpawnData spawnData = _platformGenerator.PrepareNextPlatform(cameraRightEdge);

        await UniTask.WhenAll(
            _platformAppearance.Show(spawnData),
            _cameraMovement.MoveToPlayer(transform));

        _platformGenerator.ReleasePreviousPlatform();

        _stickController.ResetStick(_platformProgress.CurrentPlatform.StickSpawnPosition);

        _playerEvents.RaiseReachedPlatform();
    }

    private async UniTask MovePlayerToFall()
    {
        Vector3 stickEnd = _stickController.GetStickEndPosition();

        Vector3 fallStartPosition = new Vector3(
            stickEnd.x,
            transform.position.y,
            transform.position.z);

        _playerAnimation.PlayRun();
        _robotAudio.StartMotor();

        await _playerMovement.MoveTo(fallStartPosition);

        _robotAudio.StopMotor();
        _playerAnimation.PlayIdle();

        _playerEvents.RaisePlayerFallStarted();

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
