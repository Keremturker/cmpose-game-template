using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Ads
{
    // What the game uses. IsAvailable is false in the editor, until consent allows ads and when it does not; premium
    // players get Skipped interstitials, Granted rewards and no banner whatever IsAvailable says.
    public interface IAds
    {
        bool IsAvailable { get; }

        // After IsAvailable changes
        event Action Changed;

        // At a natural break (a finished level); the pacer may skip it
        UniTask<AdResult> ShowInterstitialAsync(CancellationToken cancellation);

        // Give the reward for Rewarded and Granted only
        UniTask<RewardResult> ShowRewardedAsync(CancellationToken cancellation);

        // An adaptive banner at the bottom of the screen; asking while ads are off shows it once they are on
        void ShowBanner();
        void HideBanner();
    }
}
