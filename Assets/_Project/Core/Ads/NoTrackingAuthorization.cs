using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Ads
{
    // Android and the editor: there is no tracking prompt
    public sealed class NoTrackingAuthorization : ITrackingAuthorization
    {
        public TrackingStatus Status => TrackingStatus.NotApplicable;

        public UniTask<TrackingStatus> RequestAsync() => UniTask.FromResult(TrackingStatus.NotApplicable);
    }
}
