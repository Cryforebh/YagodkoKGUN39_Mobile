using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animatorPlayerVisual;

    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int PushHash = Animator.StringToHash("Push");
    private static readonly int FireHash = Animator.StringToHash("Fire");

    private UniTaskCompletionSource _pushCompletionSource;

    public void PlayIdle()
    {
        _animatorPlayerVisual.SetBool(IsRunningHash, false);
    }

    public void PlayRun()
    {
        _animatorPlayerVisual.SetBool(IsRunningHash, true);
    }

    public void PlayFire()
    {
        _animatorPlayerVisual.SetBool(IsRunningHash, false);

        _animatorPlayerVisual.SetTrigger(FireHash);
    }

    public UniTask PlayPushAsync()
    {
        _pushCompletionSource = new UniTaskCompletionSource();

        _animatorPlayerVisual.SetBool(IsRunningHash, false);
        _animatorPlayerVisual.SetTrigger(PushHash);

        return _pushCompletionSource.Task;
    }

    public void CompletePushAnimation()
    {
        _pushCompletionSource?.TrySetResult();
        _pushCompletionSource = null;
    }
}
