namespace Kturker.UnityBase.Core.Ads
{
    // Rewarded: watched to the end. Granted: premium, the reward without an ad. Dismissed: closed early, no reward.
    // NotReady / Failed as in AdResult; only Rewarded and Granted give the reward.
    public enum RewardResult
    {
        Rewarded,
        Granted,
        Dismissed,
        NotReady,
        Failed
    }
}
