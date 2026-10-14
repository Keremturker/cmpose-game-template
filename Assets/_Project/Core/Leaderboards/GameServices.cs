using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Preferences;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // Pure logic over the platform gateway. Start signs in silently and never holds up the boot; a score, an unlock or
    // a few achievement steps asked for while signed out (or when a send fails) are kept in preferences and sent when
    // the player is signed in, once. One round of sending runs at a time; calls made during a round join it.
    public sealed class GameServices : IGameServices, IStartable, IDisposable
    {
        public const string PlaceholderWarning = "Game services use placeholder IDs: set them in LeaderboardsConfig (see README)";

        private const string BestKey = "gameServices.best";
        private const string SentKey = "gameServices.sent";
        private const string PendingUnlocksKey = "gameServices.unlocks";
        private const string SentUnlocksKey = "gameServices.unlocked";
        private const string StepKeysKey = "gameServices.stepKeys";
        private const string StepsPrefix = "gameServices.steps.";

        private readonly IGameServicesGateway gateway;
        private readonly IPreferences preferences;
        private readonly ILog log;
        private readonly bool configured;
        private readonly bool warnPlaceholders;
        private bool signedIn;
        private bool signingIn;
        private bool screenOpen;
        private bool syncing;
        private bool dirty;
        private bool disposed;
        private UniTaskCompletionSource syncDone;

        public GameServices(IGameServicesGateway gateway, IPreferences preferences, ILog log, bool configured, bool warnPlaceholders)
        {
            this.gateway = gateway;
            this.preferences = preferences;
            this.log = log;
            this.configured = configured;
            this.warnPlaceholders = warnPlaceholders;
        }

        public bool IsAvailable => configured;
        public bool IsSignedIn => signedIn;

        public event Action Changed;

        public void Start() => RunAsync().Forget();

        public void Dispose() => disposed = true;

        public async UniTask<GameServicesResult> SignInAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!configured) return GameServicesResult.Unavailable;
            if (signedIn) return GameServicesResult.Ok;
            if (signingIn) return GameServicesResult.Failed;
            signingIn = true;
            bool ok;
            try
            {
                ok = await gateway.SignInAsync();
            }
            finally
            {
                signingIn = false;
            }
            if (disposed) return GameServicesResult.Failed;
            if (!ok) return GameServicesResult.NotSignedIn;
            signedIn = true;
            Changed?.Invoke();
            SyncAsync().Forget();
            return GameServicesResult.Ok;
        }

        public async UniTask<GameServicesResult> SubmitScoreAsync(long score, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            GameServicesResult result;
            if (!configured) result = GameServicesResult.Unavailable;
            else
            {
                var best = Best;
                if (!best.HasValue || score > best.Value) WriteLong(BestKey, score);
                var sent = Sent;
                if (sent.HasValue && score <= sent.Value) result = GameServicesResult.Ok;
                else if (!signedIn) result = GameServicesResult.NotSignedIn;
                else
                {
                    await SyncAsync();
                    sent = Sent;
                    result = sent.HasValue && score <= sent.Value ? GameServicesResult.Ok : GameServicesResult.Failed;
                }
            }
            log.Marker($"CMPOSE_SCORE result={result}");
            return result;
        }

        public async UniTask<GameServicesResult> UnlockAchievementAsync(string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            GameServicesResult result;
            if (!configured) result = GameServicesResult.Unavailable;
            else if (!ValidKey(key)) result = GameServicesResult.Failed;
            else if (Has(SentUnlocksKey, key)) result = GameServicesResult.Ok;
            else
            {
                Add(PendingUnlocksKey, key);
                if (!signedIn) result = GameServicesResult.NotSignedIn;
                else
                {
                    await SyncAsync();
                    result = Has(SentUnlocksKey, key) ? GameServicesResult.Ok : GameServicesResult.Failed;
                }
            }
            log.Marker($"CMPOSE_ACHIEVEMENT key={key} result={result}");
            return result;
        }

        public async UniTask<GameServicesResult> IncrementAchievementAsync(string key, int steps, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            GameServicesResult result;
            if (!configured) result = GameServicesResult.Unavailable;
            else if (!ValidKey(key) || steps <= 0) result = GameServicesResult.Failed;
            else
            {
                SetSteps(key, Steps(key) + steps);
                Add(StepKeysKey, key);
                if (!signedIn) result = GameServicesResult.NotSignedIn;
                else
                {
                    await SyncAsync();
                    result = Steps(key) == 0 ? GameServicesResult.Ok : GameServicesResult.Failed;
                }
            }
            log.Marker($"CMPOSE_ACHIEVEMENT key={key} result={result}");
            return result;
        }

        public UniTask<GameServicesResult> ShowLeaderboardAsync(CancellationToken token) =>
            ShowAsync(g => g.ShowLeaderboardAsync(), token);

        public UniTask<GameServicesResult> ShowAchievementsAsync(CancellationToken token) =>
            ShowAsync(g => g.ShowAchievementsAsync(), token);

        private async UniTaskVoid RunAsync()
        {
            if (!configured)
            {
                if (warnPlaceholders) log.Warning(PlaceholderWarning);
                Mark();
                return;
            }
            signedIn = await gateway.StartAsync();
            if (disposed) return;
            Mark();
            Changed?.Invoke();
            if (signedIn) await SyncAsync();
        }

        // A screen needs a signed-in player: ask to sign in first, stop if the player declines. One screen at a time.
        private async UniTask<GameServicesResult> ShowAsync(Func<IGameServicesGateway, UniTask<bool>> show, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!configured) return GameServicesResult.Unavailable;
            if (screenOpen) return GameServicesResult.Failed;
            screenOpen = true;
            try
            {
                if (!signedIn)
                {
                    var signIn = await SignInAsync(token);
                    if (signIn != GameServicesResult.Ok) return signIn;
                }
                return await show(gateway) ? GameServicesResult.Ok : GameServicesResult.Failed;
            }
            finally
            {
                screenOpen = false;
            }
        }

        // Sends what is kept. A call that arrives while a round runs marks it dirty and waits for the round, which
        // then goes round again; a failed send ends the round (the values stay for the next call).
        private async UniTask SyncAsync()
        {
            dirty = true;
            if (syncing)
            {
                await syncDone.Task;
                return;
            }
            syncing = true;
            syncDone = new UniTaskCompletionSource();
            try
            {
                while (dirty && signedIn && !disposed)
                {
                    dirty = false;
                    if (!await SendOnceAsync()) break;
                }
            }
            finally
            {
                dirty = false;
                syncing = false;
                syncDone.TrySetResult();
            }
        }

        private async UniTask<bool> SendOnceAsync()
        {
            var ok = true;
            var best = Best;
            var sent = Sent;
            if (best.HasValue && (!sent.HasValue || best.Value > sent.Value))
            {
                if (await gateway.SubmitScoreAsync(best.Value)) WriteLong(SentKey, best.Value);
                else ok = false;
            }
            foreach (var key in Keys(PendingUnlocksKey))
            {
                if (await gateway.UnlockAsync(key))
                {
                    Remove(PendingUnlocksKey, key);
                    Add(SentUnlocksKey, key);
                }
                else ok = false;
            }
            foreach (var key in Keys(StepKeysKey))
            {
                var steps = Steps(key);
                if (steps <= 0)
                {
                    Remove(StepKeysKey, key);
                    continue;
                }
                if (!await gateway.IncrementAsync(key, steps))
                {
                    ok = false;
                    continue;
                }
                var left = Steps(key) - steps;
                SetSteps(key, left < 0 ? 0 : left);
                if (left <= 0) Remove(StepKeysKey, key);
            }
            return ok;
        }

        private void Mark() =>
            log.Marker($"CMPOSE_GAMES available={(configured ? "true" : "false")} signedIn={(signedIn ? "true" : "false")}");

        private static bool ValidKey(string key) => !string.IsNullOrEmpty(key) && key.IndexOf(',') < 0;

        private long? Best => ReadLong(BestKey);
        private long? Sent => ReadLong(SentKey);

        private long? ReadLong(string key) =>
            long.TryParse(preferences.GetString(key, string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : (long?)null;

        private void WriteLong(string key, long value)
        {
            preferences.SetString(key, value.ToString(CultureInfo.InvariantCulture));
            preferences.Save();
        }

        private List<string> Keys(string setKey)
        {
            var text = preferences.GetString(setKey, string.Empty);
            return string.IsNullOrEmpty(text) ? new List<string>() : new List<string>(text.Split(','));
        }

        private bool Has(string setKey, string key) => Keys(setKey).Contains(key);

        private void Add(string setKey, string key)
        {
            var keys = Keys(setKey);
            if (keys.Contains(key)) return;
            keys.Add(key);
            preferences.SetString(setKey, string.Join(",", keys));
            preferences.Save();
        }

        private void Remove(string setKey, string key)
        {
            var keys = Keys(setKey);
            if (!keys.Remove(key)) return;
            preferences.SetString(setKey, string.Join(",", keys));
            preferences.Save();
        }

        private int Steps(string key) => preferences.GetInt(StepsPrefix + key, 0);

        private void SetSteps(string key, int value)
        {
            preferences.SetInt(StepsPrefix + key, value);
            preferences.Save();
        }
    }
}
