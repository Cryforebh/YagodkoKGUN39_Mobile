using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface ITimeModel : IDisposable
{
    IReadOnlyReactiveProperty<int> Time { get; }

    UniTask StartTimer();
}
