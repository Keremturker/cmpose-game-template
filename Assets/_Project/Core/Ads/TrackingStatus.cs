namespace Kturker.UnityBase.Core.Ads
{
    // App Tracking Transparency; NotApplicable on Android and in the editor
    public enum TrackingStatus
    {
        NotApplicable,
        NotDetermined,
        Restricted,
        Denied,
        Authorized
    }
}
