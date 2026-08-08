using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformRepository : MonoBehaviour
{
    [SerializeField] private Transform _platformsRoot;

    private readonly List<PlatformView> _platforms = new();

    public IReadOnlyList<PlatformView> Platforms => _platforms;

    private void Awake()
    {
        _platforms.Clear();

        foreach (Transform child in _platformsRoot)
        {
            PlatformView platform = child.GetComponent<PlatformView>();

            if (platform != null)
            {
                _platforms.Add(platform);
            }
        }

        _platforms.Sort((a, b) =>
            a.transform.position.x.CompareTo(b.transform.position.x));

        Debug.Log($"PlatformRepository: найдено платформ: {_platforms.Count}");
    }
}
