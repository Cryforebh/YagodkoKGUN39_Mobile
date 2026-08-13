using Cysharp.Threading.Tasks;
using System;
using UniRx;

public interface ITimeModel : IDisposable
{
    IReadOnlyReactiveProperty<int> Time { get; }

    UniTask StartTimer();
}
