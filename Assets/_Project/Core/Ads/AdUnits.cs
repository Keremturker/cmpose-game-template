namespace Kturker.UnityBase.Core.Ads
{
    // The three ad unit IDs of one platform
    public readonly struct AdUnits
    {
        public AdUnits(string banner, string interstitial, string rewarded)
        {
            Banner = banner;
            Interstitial = interstitial;
            Rewarded = rewarded;
        }

        public string Banner { get; }
        public string Interstitial { get; }
        public string Rewarded { get; }
    }
}
