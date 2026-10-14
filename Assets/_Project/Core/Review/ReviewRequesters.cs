using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Review
{
    public static class ReviewRequesters
    {
        public static IReviewRequester ForPlatform(ILog log)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new PlayReviewRequester(log);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosReviewRequester(log);
#else
            return new LogReviewRequester(log);
#endif
        }
    }
}
