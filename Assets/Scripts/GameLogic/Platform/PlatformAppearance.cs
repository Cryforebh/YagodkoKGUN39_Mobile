using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class PlatformAppearance : MonoBehaviour
{
    [SerializeField] private Camera _gameCamera;
    [SerializeField, Min(0.1f)] private float _moveSpeed = 18f;
    [SerializeField, Min(0f)] private float _offscreenOffset = 0.5f;

    private CancellationToken _cancellationToken;

    private void Awake()
    {
        _cancellationToken =
            this.GetCancellationTokenOnDestroy();
    }

    public UniTask Show(PlatformSpawnData spawnData)
    {
        if (!spawnData.IsValid)
            return UniTask.CompletedTask;

        PlatformView platform = spawnData.Platform;

        float cameraRightEdge = _gameCamera
            .ViewportToWorldPoint(
                new Vector3(1f, 0.5f, 0f))
            .x;

        float startX =
            cameraRightEdge +
            _offscreenOffset +
            platform.PivotToLeftEdge;

        Vector3 startPosition = new Vector3(
            startX,
            spawnData.TargetPosition.y,
            spawnData.TargetPosition.z);

        platform.Activate(startPosition);

        return platform.MoveTo(
            spawnData.TargetPosition,
            _moveSpeed,
            _cancellationToken);
    }
}
