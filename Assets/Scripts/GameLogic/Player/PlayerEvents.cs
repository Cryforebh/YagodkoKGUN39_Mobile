using System;
using UniRx;

public class PlayerEvents
{
    private readonly Subject<Unit> _reachedPlatform = new();
    private readonly Subject<Unit> _playerFell = new();
    private readonly Subject<Unit> _playerFallCompleted = new();

    public IObservable<Unit> ReachedPlatform => _reachedPlatform;
    public IObservable<Unit> PlayerFell => _playerFell;
    public IObservable<Unit> PlayerFallCompleted => _playerFallCompleted;

    public void RaiseReachedPlatform()
    {
        _reachedPlatform.OnNext(Unit.Default);
    }

    public void RaisePlayerFell()
    {
        _playerFell.OnNext(Unit.Default);
    }

    public void RaisePlayerFallCompleted()
    {
        _playerFallCompleted.OnNext(Unit.Default);
    }
}