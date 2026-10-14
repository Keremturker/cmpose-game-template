namespace Kturker.UnityBase.Core.Ads
{
    // Shown: the player saw it. Skipped: premium or the pacer said not now. NotReady: no ad loaded, ads off, or
    // another full-screen ad is open. Failed: the SDK could not show it.
    public enum AdResult
    {
        Shown,
        Skipped,
        NotReady,
        Failed
    }
}
