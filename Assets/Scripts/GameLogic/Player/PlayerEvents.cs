using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerEvents
{
    private readonly Subject<Unit> _reachedPlatform = new();
    private readonly Subject<Unit> _playerFell = new();

    public IObservable<Unit> ReachedPlatform => _reachedPlatform;
    public IObservable<Unit> PlayerFell => _playerFell;

    public void RaiseReachedPlatform()
    {
        _reachedPlatform.OnNext(Unit.Default);
    }

    public void RaisePlayerFell()
    {
        _playerFell.OnNext(Unit.Default);
    }
}