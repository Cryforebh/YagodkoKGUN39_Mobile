using UnityEngine;

public class PlatformProgress : MonoBehaviour
{
    public PlatformView PreviousPlatform { get; private set; }
    public PlatformView CurrentPlatform { get; private set; }
    public PlatformView NextPlatform { get; private set; }

    public bool HasNextPlatform => NextPlatform != null;

    public void Initialize(PlatformView currentPlatform, PlatformView nextPlatform)
    {
        PreviousPlatform = null;
        CurrentPlatform = currentPlatform;
        NextPlatform = nextPlatform;
    }

    public void MoveToNextPlatform()
    {
        if (!HasNextPlatform)
            return;

        PreviousPlatform = CurrentPlatform;
        CurrentPlatform = NextPlatform;
        NextPlatform = null;
    }

    public void SetNextPlatform(PlatformView platform)
    {
        NextPlatform = platform;
    }

    public void ClearPreviousPlatform()
    {
        PreviousPlatform = null;
    }
}
