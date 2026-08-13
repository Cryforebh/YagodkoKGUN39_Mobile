using UnityEngine;

public readonly struct PlatformSpawnData
{
    public PlatformView Platform { get; }
    public Vector3 TargetPosition { get; }

    public bool IsValid => Platform != null;

    public PlatformSpawnData(
        PlatformView platform,
        Vector3 targetPosition)
    {
        Platform = platform;
        TargetPosition = targetPosition;
    }
}
