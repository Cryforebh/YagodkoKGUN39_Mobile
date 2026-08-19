using UnityEngine;

public class RobotAudio : MonoBehaviour
{
    private enum MotorState
    {
        Stopped,
        Looping,
        WaitingForOutro,
        Finishing
    }

    [Header("Sources")]
    [SerializeField] private AudioSource _motorSource;
    [SerializeField] private AudioSource _effectsSource;
    [SerializeField] private AudioSource _bodySource;

    [Header("Motor")]
    [SerializeField] private AudioClip _motorClip;

    [Tooltip("Начало зацикленного участка в секундах")]
    [SerializeField, Min(0f)]
    private float _loopStartTime;

    [Tooltip("Конец зацикленного участка в секундах")]
    [SerializeField, Min(0f)]
    private float _loopEndTime;

    [Header("Other sounds")]
    [SerializeField] private AudioClip _fallClip;
    [SerializeField] private AudioClip _stickCreateClip;
    [SerializeField] private AudioClip _handActive;

    private MotorState _motorState = MotorState.Stopped;

    private void Awake()
    {
        PrepareSource(_motorSource);
        PrepareSource(_effectsSource);
        PrepareSource(_bodySource);
    }

    private void Update()
    {
        UpdateMotor();
    }

    public void StartCreateStick()
    {
        if (_stickCreateClip == null)
            return;

        _effectsSource.Stop();

        _effectsSource.clip = _stickCreateClip;
        _effectsSource.loop = true;
        _effectsSource.Play();
    }

    public void StopCreateStick()
    {
        _effectsSource.loop = false;
        _effectsSource.Stop();

        _bodySource.Stop();

        _bodySource.clip = _handActive;
        _bodySource.Play();
    }

    public void StartMotor()
    {
        if (_motorClip == null)
            return;

        if (!HasValidLoop())
        {
            Debug.LogError(
                "Некорректно настроены границы цикла моторчика.");

            return;
        }

        _motorSource.Stop();

        _motorSource.clip = _motorClip;
        _motorSource.time = 0f;
        _motorSource.Play();

        _motorState = MotorState.Looping;
    }

    public void StopMotor()
    {
        if (_motorState != MotorState.Looping)
            return;

        // Если движение закончилось ещё во вступлении,
        // доигрываем вступление и сразу переходим к концовке.
        if (_motorSource.time < _loopStartTime)
        {
            _motorState =
                MotorState.WaitingForOutro;

            return;
        }

        // Если сейчас проигрывается зацикленная часть,
        // больше не возвращаемся к её началу.
        // Звук дойдёт до финальной части самостоятельно.
        _motorState = MotorState.Finishing;
    }

    public void PlayFall()
    {
        if (_fallClip == null)
            return;

        _effectsSource.PlayOneShot(_fallClip);
    }

    private void UpdateMotor()
    {
        if (_motorState == MotorState.Stopped)
            return;

        if (_motorState == MotorState.Looping)
        {
            if (!_motorSource.isPlaying)
            {
                _motorSource.time = _loopStartTime;
                _motorSource.Play();
                return;
            }

            RepeatLoopSection();
            return;
        }

        if (!_motorSource.isPlaying)
        {
            _motorState = MotorState.Stopped;
            return;
        }

        if (_motorState == MotorState.WaitingForOutro)
            SkipLoopSectionAfterIntro();
    }

    private void RepeatLoopSection()
    {
        if (_motorSource.time < _loopEndTime)
            return;

        float overshoot =
            _motorSource.time - _loopEndTime;

        _motorSource.time = Mathf.Min(
            _loopStartTime + overshoot,
            _loopEndTime);
    }

    private void SkipLoopSectionAfterIntro()
    {
        if (_motorSource.time < _loopStartTime)
            return;

        _motorSource.time = _loopEndTime;
        _motorState = MotorState.Finishing;
    }

    private bool HasValidLoop()
    {
        return _loopStartTime >= 0f &&
               _loopEndTime > _loopStartTime &&
               _loopEndTime < _motorClip.length;
    }

    private static void PrepareSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }
}
