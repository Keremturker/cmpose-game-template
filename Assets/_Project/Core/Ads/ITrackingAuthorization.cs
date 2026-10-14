using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Ads
{
    // The ATT status and the system prompt (iOS 14.5+)
    public interface ITrackingAuthorization
    {
        TrackingStatus Status { get; }

        // Shows the prompt and waits for the answer
        UniTask<TrackingStatus> RequestAsync();
    }
}
