using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISceneLoader
{
    UniTask LoadMain();
    UniTask LoadGame();
    UniTask RestartGame();
}
