using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class FrameRateController : MonoBehaviour
{
    private const int TargetFrameRate = 60;

    private void Awake()
    {
        ApplyFrameRate();
    }

    private void Start()
    {
        ApplyFrameRateAfterInitialization().Forget();
    }

    private async UniTaskVoid ApplyFrameRateAfterInitialization()
    {
        await UniTask.Yield();

        ApplyFrameRate();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyFrameRate();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (!isPaused)
            ApplyFrameRate();
    }

    private void ApplyFrameRate()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;

        Debug.Log(
            $"Target FPS установлен: " +
            $"{Application.targetFrameRate}");
    }
}
