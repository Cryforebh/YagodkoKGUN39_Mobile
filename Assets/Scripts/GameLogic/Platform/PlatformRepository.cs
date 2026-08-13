using System.Collections.Generic;
using UnityEngine;

public class PlatformRepository : MonoBehaviour
{
    // ¡ŒÀ≈≈ Õ≈ »—œŒÀ‹«”≈“—ﬂ!!! Õ≈ «¿¡€“‹ ”ƒ¿À»“‹!!!

    private readonly List<PlatformView> _platforms = new();

    public IReadOnlyList<PlatformView> Platforms => _platforms;

    public void Add(PlatformView platform)
    {
        if (platform == null)
            return;

        if (_platforms.Contains(platform))
            return;

        _platforms.Add(platform);
    }

    public bool Replace(PlatformView previousPlatform, PlatformView newPlatform)
    {
        if (previousPlatform == null || newPlatform == null)
            return false;

        int index = _platforms.IndexOf(previousPlatform);

        if (index < 0)
            return false;

        _platforms[index] = newPlatform;
        return true;
    }

    public void Clear()
    {
        _platforms.Clear();
    }
}
