using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class PlatformUIPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _platformCountText;

    [Inject] private PlatformProgress _platformProgress;
    [Inject] private PlayerEvents _playerEvents;

    private readonly CompositeDisposable _disposables = new();

    private void Start()
    {
        UpdatePlatformCount();

        _playerEvents.ReachedPlatform
            .Subscribe(_ => UpdatePlatformCount())
            .AddTo(_disposables);
    }

    private void UpdatePlatformCount()
    {
        _platformCountText.text = $"P: {_platformProgress.PassedPlatforms.ToString()}";
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
