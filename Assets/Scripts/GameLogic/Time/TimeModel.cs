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

    public UniTask StartTimer()
    {
        return CountTime(_cancellationTokenSource.Token);
    }

    private async UniTask CountTime(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await UniTask.Delay(
                    1000,
                    cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _time.Value++;
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
            _cancellationTokenSource.Cancel();

        _cancellationTokenSource.Dispose();
        _time.Dispose();
    }
}
