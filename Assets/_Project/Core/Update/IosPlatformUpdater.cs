#if UNITY_IOS && !UNITY_EDITOR
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;

namespace Kturker.UnityBase.Core.Update
{
    // iOS has no in-app update: Update opens the App Store page from the lookup; without one it cannot do anything
    public sealed class IosPlatformUpdater : IPlatformUpdater
    {
        private readonly ILog log;

        public IosPlatformUpdater(ILog log) => this.log = log;

        public StorePlatform Platform => StorePlatform.Ios;

        public AppVersion CurrentVersion => new AppVersion(Application.version, null);

        public UniTask<bool> StartFlexibleUpdateAsync(CancellationToken cancellation) => UniTask.FromResult(false);

        public bool CanStartUpdate(string storeUrl) => !string.IsNullOrWhiteSpace(storeUrl);

        public void StartUpdate(string storeUrl)
        {
            if (!CanStartUpdate(storeUrl))
            {
                log.Warning("No App Store page for this game");
                return;
            }
            Application.OpenURL(storeUrl);
        }
    }
}
#endif
