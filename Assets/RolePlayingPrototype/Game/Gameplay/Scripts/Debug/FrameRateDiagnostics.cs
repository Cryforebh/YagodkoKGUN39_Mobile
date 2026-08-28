using System.Text;
using UnityEngine;

namespace SampleProject
{
    public sealed class FrameRateDiagnostics : MonoBehaviour
    {
        private const float MeasurementWindow = 0.5f;
        private const float WarmupDuration = 1f;
        private const float SixtyFpsFrameTime = 1f / 60f;
        private const float ThirtyFpsFrameTime = 1f / 30f;
        private const float TwentyFpsFrameTime = 1f / 20f;

        private readonly StringBuilder _text = new(256);
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private string _displayText;
        private float _warmupRemaining;
        private float _windowTime;
        private float _measurementTime;
        private float _currentFps;
        private float _minimumFps;
        private float _worstFrameTime;
        private int _windowFrames;
        private int _framesBelowSixty;
        private int _framesBelowThirty;
        private int _framesBelowTwenty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (!Application.isEditor && !Debug.isDebugBuild || FindObjectOfType<FrameRateDiagnostics>() != null)
            {
                return;
            }

            var diagnostics = new GameObject(nameof(FrameRateDiagnostics));
            DontDestroyOnLoad(diagnostics);
            diagnostics.AddComponent<FrameRateDiagnostics>();
        }

        private void Awake()
        {
            ResetMeasurement();
        }

        private void Update()
        {
            var frameTime = Time.unscaledDeltaTime;
            if (frameTime <= 0f)
            {
                return;
            }

            if (_warmupRemaining > 0f)
            {
                _warmupRemaining -= frameTime;
                if (_warmupRemaining <= 0f)
                {
                    RefreshDisplayText();
                }
                return;
            }

            _measurementTime += frameTime;
            _windowTime += frameTime;
            _windowFrames++;
            _worstFrameTime = Mathf.Max(_worstFrameTime, frameTime);
            if (frameTime > SixtyFpsFrameTime)
            {
                _framesBelowSixty++;
            }

            if (frameTime > ThirtyFpsFrameTime)
            {
                _framesBelowThirty++;
            }

            if (frameTime > TwentyFpsFrameTime)
            {
                _framesBelowTwenty++;
            }

            if (_windowTime < MeasurementWindow)
            {
                return;
            }

            _currentFps = _windowFrames / _windowTime;
            _minimumFps = Mathf.Min(_minimumFps, _currentFps);
            _windowTime = 0f;
            _windowFrames = 0;
            RefreshDisplayText();
        }

        private void OnGUI()
        {
            EnsureStyles();
            var scale = Mathf.Clamp(Screen.height / 1080f, 0.75f, 1.5f);
            var width = 340f;
            var height = 230f;
            var margin = 18f;
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var scaledScreenWidth = Screen.width / scale;
            var scaledScreenHeight = Screen.height / scale;
            var area = new Rect(scaledScreenWidth - width - margin, scaledScreenHeight - height - margin, width, height);
            GUI.Box(area, GUIContent.none);

            GUI.Label(new Rect(area.x + 12f, area.y + 8f, area.width - 24f, 170f), _displayText, _labelStyle);
            if (GUI.Button(new Rect(area.x + 12f, area.yMax - 44f, area.width - 24f, 32f), "Сбросить замер", _buttonStyle))
            {
                ResetMeasurement();
            }

            GUI.matrix = previousMatrix;
        }

        private void ResetMeasurement()
        {
            _warmupRemaining = WarmupDuration;
            _windowTime = 0f;
            _measurementTime = 0f;
            _currentFps = 0f;
            _minimumFps = float.MaxValue;
            _worstFrameTime = 0f;
            _windowFrames = 0;
            _framesBelowSixty = 0;
            _framesBelowThirty = 0;
            _framesBelowTwenty = 0;
            RefreshDisplayText();
        }

        private void RefreshDisplayText()
        {
            _text.Clear();
            _text.AppendLine("FPS DIAGNOSTICS");
            if (_warmupRemaining > 0f)
            {
                _text.AppendLine("Подготовка замера...");
                _displayText = _text.ToString();
                return;
            }

            _text.Append("Текущий FPS (0.5 c): ").AppendLine(_currentFps.ToString("F1"));
            _text.Append("Минимальный FPS: ").AppendLine((_minimumFps == float.MaxValue ? 0f : _minimumFps).ToString("F1"));
            _text.Append("Худший кадр: ").Append((_worstFrameTime * 1000f).ToString("F1")).AppendLine(" ms");
            _text.Append("Кадров > 16.7 ms: ").AppendLine(_framesBelowSixty.ToString());
            _text.Append("Кадров > 33.3 ms: ").AppendLine(_framesBelowThirty.ToString());
            _text.Append("Кадров > 50.0 ms: ").AppendLine(_framesBelowTwenty.ToString());
            _text.Append("Время теста: ").Append(_measurementTime.ToString("F1")).AppendLine(" c");
            _displayText = _text.ToString();
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.UpperLeft
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18
            };
        }
    }
}
