using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _shiftX = -2f;

    private Camera _camera;

    private float _defaultY;
    private float _defaultZ;

    private CancellationToken _cancellationToken;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _defaultY = transform.position.y;
        _defaultZ = transform.position.z;

        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public float GetRightEdgeAfterMove(Transform player)
    {
        float targetCameraX = player.position.x - _shiftX;

        float halfVisibleWidth = _camera.orthographicSize * _camera.aspect;

        return targetCameraX + halfVisibleWidth;
    }

    public async UniTask MoveToPlayer(Transform player)
    {
        Vector3 targetPosition = new Vector3(
            player.position.x - _shiftX,
            _defaultY,
            _defaultZ);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime);

            await UniTask.Yield(
                cancellationToken: _cancellationToken);
        }

        transform.position = targetPosition;
    }
}
