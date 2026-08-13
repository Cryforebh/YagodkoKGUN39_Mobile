using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class PlatformView : MonoBehaviour
{
    [SerializeField] private float _shiftNextPoint = 0.35f;

    private BoxCollider2D _collider;

    public float LeftEdge => _collider.bounds.min.x;
    public float RightEdge => _collider.bounds.max.x;
    public float TopEdge => _collider.bounds.max.y;

    public float PivotToLeftEdge
    {
        get
        {
            float scaleX = Mathf.Abs(transform.lossyScale.x);
            return (_collider.size.x * 0.5f - _collider.offset.x) * scaleX;
        }
    }

    public float PivotToRightEdge
    {
        get
        {
            float scaleX = Mathf.Abs(transform.lossyScale.x);
            return (_collider.size.x * 0.5f + _collider.offset.x) * scaleX;
        }
    }

    public Vector3 StickSpawnPosition => new Vector3(
        RightEdge,
        TopEdge,
        transform.position.z);

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    public Vector3 GetLandingPosition(float playerOffset)
    {
        return new Vector3(
            RightEdge - _shiftNextPoint,
            TopEdge + playerOffset,
            transform.position.z);
    }

    public void Activate(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public async UniTask MoveTo(Vector3 targetPosition, float speed, CancellationToken cancellationToken)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            cancellationToken.ThrowIfCancellationRequested();

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * UnityEngine.Time.deltaTime);

            await UniTask.Yield(cancellationToken: cancellationToken);
        }

        transform.position = targetPosition;
    }
}