using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _fallSpeed = 20f;
    [SerializeField] private float _fallDistance = 5f;

    //private Rigidbody2D _body;
    private CancellationToken _cancellationToken;

    private void Awake()
    {
        //_body = GetComponent<Rigidbody2D>();
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    public async UniTask MoveTo(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime);

            await UniTask.Yield(cancellationToken: _cancellationToken);
        }

        transform.position = targetPosition;
    }

    public async UniTask FallAsync()
    {
        Vector3 targetPosition = transform.position + Vector3.down * _fallDistance;

        //_body.isKinematic = true;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _fallSpeed * Time.deltaTime);

            await UniTask.Yield(cancellationToken: _cancellationToken);
        }

        transform.position = targetPosition;
    }
}
