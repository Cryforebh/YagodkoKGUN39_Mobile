using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

    private Animator _animator;

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
}
