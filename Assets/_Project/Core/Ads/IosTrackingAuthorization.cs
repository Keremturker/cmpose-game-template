#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Ads
{
    // ATTrackingManager through Plugins/iOS/CmposeTracking.mm. The system answers on its own queue, so the request
    // polls the bridge's done flag and every call stays on Unity's main thread.
    public sealed class IosTrackingAuthorization : ITrackingAuthorization
    {
        [DllImport("__Internal")]
        private static extern int CmposeTrackingStatus();

        [DllImport("__Internal")]
        private static extern void CmposeTrackingRequest();

        [DllImport("__Internal")]
        private static extern int CmposeTrackingDone();

        public TrackingStatus Status => TrackingAuthorizations.FromAtt(CmposeTrackingStatus());

        public async UniTask<TrackingStatus> RequestAsync()
        {
            CmposeTrackingRequest();
            while (CmposeTrackingDone() == 0) await UniTask.Delay(200, ignoreTimeScale: true);
            return Status;
        }
    }
}
#endif
