namespace Kturker.UnityBase.Core.Ads
{
    public static class TrackingAuthorizations
    {
        public static ITrackingAuthorization ForPlatform()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new IosTrackingAuthorization();
#else
            return new NoTrackingAuthorization();
#endif
        }

        // ATTrackingManagerAuthorizationStatus: 0 not determined, 1 restricted, 2 denied, 3 authorized
        public static TrackingStatus FromAtt(int status)
        {
            switch (status)
            {
                case 1: return TrackingStatus.Restricted;
                case 2: return TrackingStatus.Denied;
                case 3: return TrackingStatus.Authorized;
                default: return TrackingStatus.NotDetermined;
            }
        }
    }
}
