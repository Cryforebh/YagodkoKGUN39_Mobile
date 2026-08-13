using System.Collections.Generic;
using UnityEngine;

public class PlatformPool : MonoBehaviour
{
    [SerializeField] private List<PlatformView> _platformPrefabs;
    [SerializeField] private Transform _poolRoot;
    [Tooltip("Определяет сколько похожих экземпляров платформ может быть в сцене одновременно.")]
    [SerializeField, Min(1)] private int _instancesPerPrefab = 2;

    private readonly List<PlatformView> _availablePlatforms = new();
    private readonly List<PlatformView> _usedPlatforms = new();

    // Позволяет определить, из какого префаба создан каждый экземпляр.
    private readonly Dictionary<PlatformView, PlatformView> _sourcePrefabs = new();

    public int AvailableCount => _availablePlatforms.Count;

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        foreach (PlatformView prefab in _platformPrefabs)
        {
            if (prefab == null)
                continue;

            for (int i = 0; i < _instancesPerPrefab; i++)
            {
                PlatformView platform = Instantiate(prefab, _poolRoot);

                platform.Deactivate();

                _availablePlatforms.Add(platform);
                _sourcePrefabs.Add(platform, prefab);
            }
        }

        Debug.Log(
            $"Создано платформ: {_availablePlatforms.Count}");
    }

    public PlatformView TakeRandom()
    {
        if (_availablePlatforms.Count == 0)
        {
            Debug.LogError("Свободные платформы закончились.");
            return null;
        }

        int randomIndex = Random.Range(0, _availablePlatforms.Count);
        PlatformView platform = _availablePlatforms[randomIndex];

        MarkAsUsed(platform);

        return platform;
    }

    public PlatformView Take(PlatformView requestedPrefab)
    {
        if (requestedPrefab == null)
        {
            Debug.LogError("Не указан префаб платформы.");
            return null;
        }

        foreach (PlatformView platform in _availablePlatforms)
        {
            if (_sourcePrefabs[platform] != requestedPrefab)
                continue;

            MarkAsUsed(platform);
            return platform;
        }

        Debug.LogError($"Нет свободного экземпляра {requestedPrefab.name}.");

        return null;
    }

    public void Return(PlatformView platform)
    {
        if (platform == null)
            return;

        if (!_usedPlatforms.Remove(platform))
            return;

        platform.Deactivate();
        platform.transform.SetParent(_poolRoot);

        _availablePlatforms.Add(platform);
    }

    private void MarkAsUsed(PlatformView platform)
    {
        _availablePlatforms.Remove(platform);
        _usedPlatforms.Add(platform);
    }
}
