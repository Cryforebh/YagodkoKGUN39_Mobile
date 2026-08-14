using TMPro;
using UnityEngine;

public class RuntimeFpsCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text _fpsText;

    private float _elapsedTime;
    private int _frameCount;

    private void Update()
    {
        _elapsedTime += Time.unscaledDeltaTime;
        _frameCount++;

        if (_elapsedTime < 0.5f)
            return;

        float fps = _frameCount / _elapsedTime;
        double refreshRate =
            Screen.currentResolution.refreshRateRatio.value;

        _fpsText.text =
            $"FPS: {fps:0}\n" +
            $"Target: {Application.targetFrameRate}\n" +
            $"Display: {refreshRate} Hz";

        _elapsedTime = 0f;
        _frameCount = 0;
    }
}
