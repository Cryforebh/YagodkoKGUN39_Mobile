using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int PushHash = Animator.StringToHash("Push");

    private Animator _animator;

    private UniTaskCompletionSource _pushCompletionSource;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        _animator.SetBool(IsRunningHash, false);
    }

    public void PlayRun()
    {
        _animator.SetBool(IsRunningHash, true);
    }

    public UniTask PlayPushAsync()
    {
        _pushCompletionSource = new UniTaskCompletionSource();

        _animator.SetBool(IsRunningHash, false);
        _animator.SetTrigger(PushHash);

        return _pushCompletionSource.Task;
    }

    public void CompletePushAnimation()
    {
        _pushCompletionSource?.TrySetResult();
        _pushCompletionSource = null;
    }
}
