using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;

public class TimeModel : ITimeModel
{
    private readonly ReactiveProperty<int> _time = new(0);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IReadOnlyReactiveProperty<int> Time => _time;

    public async UniTask StartTimer()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await UniTask.Delay(
                    1000,
                    cancellationToken: _cancellationTokenSource.Token);

                _time.Value++;
            }
        }
        catch (OperationCanceledException)
        {
            // Таймер был остановлен
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _time.Dispose();
    }
}
