using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class FrameRateController : MonoBehaviour
{
    private const int TargetFrameRate = 60;
    private const int InitializationDelayFrames = 3;

    private CancellationToken _cancellationToken;
    private int _applyRequestId;

    private void Awake()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    private void Start()
    {
        RequestFrameRateApply();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            RequestFrameRateApply();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (!isPaused)
            RequestFrameRateApply();
    }

    private void RequestFrameRateApply()
    {
        _applyRequestId++;

        ApplyAfterInitialization(_applyRequestId, _cancellationToken).Forget();
    }

    private async UniTaskVoid ApplyAfterInitialization(int requestId, CancellationToken cancellationToken)
    {
        await UniTask.DelayFrame(
            InitializationDelayFrames,
            cancellationToken: cancellationToken);

        if (requestId != _applyRequestId)
            return;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
    }
}
