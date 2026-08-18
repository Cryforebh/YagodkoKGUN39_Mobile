using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

public class StickController : MonoBehaviour
{
    [SerializeField] private Transform _stickPivot;
    [SerializeField] private Transform _stick;
    [SerializeField] private Transform _stickEndTarget;
    [SerializeField] private SpriteRenderer _stickRenderer;

    [SerializeField] private float _growSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 300f;
    [SerializeField] private float _maxLength = 10f;

    private bool _isGrowing;
    private bool _isRotating;
    private bool _canGrow;

    private Vector2 _defaultSize;
    private Vector3 _defaultPosition;

    private CancellationToken _cancellationToken;

    private readonly Subject<Unit> _stickReady = new();

    private readonly Subject<Unit> _growthStarted = new();
    private readonly Subject<Unit> _growthFinished = new();

    public IObservable<Unit> StickReady => _stickReady;
    public IObservable<Unit> GrowthStarted => _growthStarted;
    public IObservable<Unit> GrowthFinished => _growthFinished;

    private void Awake()
    {
        _cancellationToken =
            this.GetCancellationTokenOnDestroy();

        _defaultSize = _stickRenderer.size;
        _defaultPosition = _stick.localPosition;

        ResetStick(_stickPivot.position);
    }

    private void Update()
    {
        if (_isGrowing)
            GrowStick();

        if (_isRotating)
            RotateStick();
    }

    public void BeginGrowth()
    {
        if (!_canGrow)
            return;

        _isGrowing = true;

        _growthStarted.OnNext(Unit.Default);
    }

    public void EndGrowth()
    {
        if (!_isGrowing)
            return;

        _isGrowing = false;
        _canGrow = false;

        _growthFinished.OnNext(Unit.Default);
    }

    public void StartRotation()
    {
        if (_isGrowing || _isRotating)
            return;

        _isRotating = true;
    }

    private void GrowStick()
    {
        Vector2 size = _stickRenderer.size;

        size.y = Mathf.Min(
            size.y + _growSpeed * Time.deltaTime,
            _maxLength);

        _stickRenderer.size = size;

        UpdateStickEndTarget();
    }

    private void UpdateStickEndTarget()
    {
        Vector3 position =
            _stickEndTarget.localPosition;

        position.x = 0f;
        position.y = _stickRenderer.size.y;

        _stickEndTarget.localPosition = position;
    }

    private void RotateStick()
    {
        float angle =
            _stickPivot.localEulerAngles.z;

        if (angle > 180f)
            angle -= 360f;

        angle -= _rotationSpeed * Time.deltaTime;

        if (angle <= -90f)
        {
            angle = -90f;
            _isRotating = false;

            _stickReady.OnNext(Unit.Default);
        }

        _stickPivot.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    public async UniTask LowerAsync()
    {
        const float targetAngle = -180f;

        while (true)
        {
            _cancellationToken
                .ThrowIfCancellationRequested();

            float angle =
                _stickPivot.localEulerAngles.z;

            if (angle > 180f)
                angle -= 360f;

            angle = Mathf.MoveTowards(
                angle,
                targetAngle,
                _rotationSpeed * Time.deltaTime);

            _stickPivot.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            if (Mathf.Approximately(
                    angle,
                    targetAngle))
            {
                break;
            }

            await UniTask.Yield(
                cancellationToken:
                _cancellationToken);
        }
    }

    public void ResetStick(Vector3 position)
    {
        _isGrowing = false;
        _isRotating = false;
        _canGrow = true;

        _stickRenderer.size = _defaultSize;
        _stick.localPosition = _defaultPosition;
        _stick.localScale = Vector3.one;

        UpdateStickEndTarget();

        _stickPivot.position = position;
        _stickPivot.localRotation = Quaternion.identity;


    }

    public Vector2 GetStickEndPosition()
    {
        return _stick.TransformPoint(
            new Vector3(0f, _stickRenderer.size.y, 0f));
    }

    private void OnDestroy()
    {
        _growthFinished.Dispose();
        _growthStarted.Dispose();
        _stickReady.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (_stick == null ||
            _stickRenderer == null)
        {
            return;
        }

        float halfLength =
            _stickRenderer.size.y * 0.5f;

        Vector2 stickEnd =
            _stick.TransformPoint(
                new Vector3(
                    0f,
                    halfLength,
                    0f));

        Gizmos.DrawSphere(stickEnd, 0.1f);
    }
}
