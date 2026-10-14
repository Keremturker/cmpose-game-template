using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;

namespace Kturker.UnityBase.Core.Update
{
    // The editor and other platforms: Android rules without a version code, so nothing is ever required; Update logs
    public sealed class NullPlatformUpdater : IPlatformUpdater
    {
        private readonly ILog log;

        public NullPlatformUpdater(ILog log) => this.log = log;

        public StorePlatform Platform => StorePlatform.Android;

        public AppVersion CurrentVersion => new AppVersion(Application.version, null);

        public UniTask<bool> StartFlexibleUpdateAsync(CancellationToken cancellation) => UniTask.FromResult(false);

        public bool CanStartUpdate(string storeUrl) => true;

        public void StartUpdate(string storeUrl) => log.Info("Update: opens the store on a device");
    }
}
