using UnityEngine;

public class FrameRateController : MonoBehaviour
{
    private const int TargetFrameRate = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
    }
}
