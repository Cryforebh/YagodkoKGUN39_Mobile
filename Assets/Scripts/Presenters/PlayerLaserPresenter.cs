using UniRx;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerLaserPresenter : MonoBehaviour
{
    [SerializeField] private StickController _stickController;
    [SerializeField] private Transform _laserOrigin;
    [SerializeField] private Transform _stickEndTarget;

    private LineRenderer _lineRenderer;

    private readonly CompositeDisposable
        _disposables = new();

    private bool _isActive;

    private void Awake()
    {
        _lineRenderer =
            GetComponent<LineRenderer>();

        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = false;

        _stickController.GrowthStarted
            .Subscribe(_ => ShowLaser())
            .AddTo(_disposables);

        _stickController.GrowthFinished
            .Subscribe(_ => HideLaser())
            .AddTo(_disposables);
    }

    private void LateUpdate()
    {
        if (!_isActive)
            return;

        _lineRenderer.SetPosition(
            0,
            _laserOrigin.position);

        _lineRenderer.SetPosition(
            1,
            _stickEndTarget.position);
    }

    private void ShowLaser()
    {
        _isActive = true;
        _lineRenderer.enabled = true;

        UpdateLaserImmediately();
    }

    private void HideLaser()
    {
        _isActive = false;
        _lineRenderer.enabled = false;
    }

    private void UpdateLaserImmediately()
    {
        _lineRenderer.SetPosition(
            0,
            _laserOrigin.position);

        _lineRenderer.SetPosition(
            1,
            _stickEndTarget.position);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
