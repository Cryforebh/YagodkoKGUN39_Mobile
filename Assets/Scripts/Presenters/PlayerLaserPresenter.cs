using UniRx;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerLaserPresenter : MonoBehaviour
{
    [SerializeField] private StickController _stickController;
    [SerializeField] private Transform _laserOrigin;
    [SerializeField] private Transform _stickEndTarget;
    [SerializeField] private ParticleSystem _stickSparks;
    [SerializeField] private ParticleSystem _smoke;

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

        _stickSparks.Clear(true);
        _stickSparks.Play(true);
        _smoke.Clear(true);
        _smoke.Play(true);

        UpdateLaserImmediately();
    }

    private void HideLaser()
    {
        _isActive = false;
        _lineRenderer.enabled = false;

        _stickSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
