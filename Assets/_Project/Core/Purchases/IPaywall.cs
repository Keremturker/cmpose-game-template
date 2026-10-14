namespace Kturker.UnityBase.Core.Purchases
{
    // Opens the paywall above every scene; Features/Shell's PaywallHost draws it. Showing an open paywall does nothing.
    public interface IPaywall
    {
        void Show(PaywallSource source);
    }
}
