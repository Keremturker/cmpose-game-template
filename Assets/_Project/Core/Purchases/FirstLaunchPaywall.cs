using System;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using Kturker.UnityBase.Core.Preferences;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Purchases
{
    // Shows the paywall once, on the first menu of the first run that can sell (available, not premium). Clear the
    // app's data, or the purchases.firstShown preference, to see it again.
    public sealed class FirstLaunchPaywall : IStartable, IDisposable
    {
        public const string ShownKey = "purchases.firstShown";

        private readonly ISceneLoader loader;
        private readonly IPurchases purchases;
        private readonly IPaywall paywall;
        private readonly IPreferences preferences;

        public FirstLaunchPaywall(ISceneLoader loader, IPurchases purchases, IPaywall paywall, IPreferences preferences)
        {
            this.loader = loader;
            this.purchases = purchases;
            this.paywall = paywall;
            this.preferences = preferences;
        }

        public void Start() => loader.Loaded += OnLoaded;

        public void Dispose() => loader.Loaded -= OnLoaded;

        private void OnLoaded(SceneId scene)
        {
            if (scene != SceneId.Menu) return;
            loader.Loaded -= OnLoaded;
            if (!purchases.IsAvailable || purchases.IsPremium || preferences.GetBool(ShownKey, false)) return;
            preferences.SetBool(ShownKey, true);
            preferences.Save();
            paywall.Show(PaywallSource.First);
        }
    }
}
