using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformProgress : MonoBehaviour
{
    [SerializeField] private PlatformRepository _repository;

    private int _currentPlatformIndex;

    public PlatformView CurrentPlatform =>
        _repository.Platforms[_currentPlatformIndex];

    public PlatformView NextPlatform =>
        _repository.Platforms[_currentPlatformIndex + 1];

    public bool HasNextPlatform =>
        _currentPlatformIndex + 1 < _repository.Platforms.Count;

    public void MoveToNextPlatform()
    {
        if (!HasNextPlatform)
            return;

        _currentPlatformIndex++;
    }
}
