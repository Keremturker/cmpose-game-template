#if UNITY_IOS && !UNITY_EDITOR
using Kturker.UnityBase.Core.Domain;
using UnityEngine;

namespace Kturker.UnityBase.Core.Review
{
    // StoreKit's review dialog through Unity's Device.RequestStoreReview. Debug builds show it every time, TestFlight
    // never, App Store builds a few times a year at most.
    public sealed class IosReviewRequester : IReviewRequester
    {
        private readonly ILog log;

        public IosReviewRequester(ILog log) => this.log = log;

        public void RequestReview()
        {
            if (!UnityEngine.iOS.Device.RequestStoreReview()) log.Warning("StoreKit review dialog unavailable");
        }

        public void OpenStorePage(string url, string fallbackUrl) => Application.OpenURL(url);
    }
}
#endif
