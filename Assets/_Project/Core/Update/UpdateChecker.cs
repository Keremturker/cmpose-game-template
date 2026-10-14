using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Preferences;

namespace Kturker.UnityBase.Core.Update
{
    // Reads the minimum version file (when AppConfig.minVersionUrl is set) and, on iOS, the App Store lookup, then
    // decides. At most once a day (UpdateSchedule); a failed request counts as unknown and never forces an update.
    public sealed class UpdateChecker
    {
        public const string LastCheckKey = "update.lastCheck";
        public const string LastRequiredKey = "update.lastRequired";

        private readonly IPreferences preferences;
        private readonly IHttpText http;
        private readonly IPlatformUpdater platform;
        private readonly IClock clock;
        private readonly UpdateConfig config;
        private readonly ILog log;

        public UpdateChecker(IPreferences preferences, IHttpText http, IPlatformUpdater platform, IClock clock, UpdateConfig config, ILog log)
        {
            this.preferences = preferences;
            this.http = http;
            this.platform = platform;
            this.clock = clock;
            this.config = config;
            this.log = log;
        }

        // Null when no check is due. The time and the decision are written only when the check completes.
        public async UniTask<UpdateDecision> CheckAsync(CancellationToken cancellation)
        {
            var now = clock.NowMillis;
            if (!UpdateSchedule.IsDue(ReadLastCheck(), now, preferences.GetBool(LastRequiredKey, false)))
            {
                log.Marker("CMPOSE_UPDATE not-due");
                return null;
            }
            var minVersions = await FetchMinVersionsAsync(cancellation);
            var storeRelease = platform.Platform == StorePlatform.Ios ? await FetchStoreReleaseAsync(cancellation) : null;
            cancellation.ThrowIfCancellationRequested();
            var decision = UpdateDecider.Decide(platform.CurrentVersion, minVersions, storeRelease, platform.Platform);
            preferences.SetString(LastCheckKey, now.ToString(CultureInfo.InvariantCulture));
            preferences.SetBool(LastRequiredKey, decision.Kind == UpdateKind.Required);
            preferences.Save();
            log.Marker($"CMPOSE_UPDATE decision={decision.Kind}");
            return decision;
        }

        // Text that is not a number (or another type under the key) reads as "never checked"
        private long? ReadLastCheck() =>
            long.TryParse(preferences.GetString(LastCheckKey, string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out var millis)
                ? millis
                : (long?)null;

        private async UniTask<MinVersions> FetchMinVersionsAsync(CancellationToken cancellation)
        {
            if (config.MinVersionUrl.Length == 0) return null;
            var text = await FetchAsync(config.MinVersionUrl, cancellation);
            if (text == null) return null;
            var parsed = MinVersions.Parse(text);
            if (parsed == null) log.Warning("Minimum version file unreadable: no forced update");
            return parsed;
        }

        private async UniTask<StoreRelease> FetchStoreReleaseAsync(CancellationToken cancellation)
        {
            if (config.BundleId.Length == 0) return null;
            var text = await FetchAsync(StoreRelease.LookupUrl(config.BundleId), cancellation);
            return text == null ? null : StoreRelease.Parse(text);
        }

        // An adapter that throws counts as a failed request; cancellation still ends the check
        private async UniTask<string> FetchAsync(string url, CancellationToken cancellation)
        {
            try
            {
                return await http.GetAsync(url, cancellation);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Warning($"Update check request failed: {e.GetType().Name}");
                return null;
            }
        }
    }
}
