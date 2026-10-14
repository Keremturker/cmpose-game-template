using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Update
{
    // The version on the App Store, from Apple's public lookup (no key and no server of your own)
    public sealed class StoreRelease
    {
        // Only an App Store page is ever opened from the lookup's answer
        private const string AppStorePrefix = "https://apps.apple.com/";

        public StoreRelease(string version, string storeUrl)
        {
            Version = version;
            StoreUrl = storeUrl;
        }

        public string Version { get; }
        public string StoreUrl { get; }

        public static string LookupUrl(string bundleId) =>
            "https://itunes.apple.com/lookup?bundleId=" + Uri.EscapeDataString(bundleId ?? string.Empty);

        // Null when the game is not on the App Store (yet), the answer is broken, or its page is not an App Store URL
        public static StoreRelease Parse(string json)
        {
            if (!JsonText.TryParse(json, out var root) || !(root is Dictionary<string, object> answer)) return null;
            if (!answer.TryGetValue("results", out var list) || !(list is List<object> results) || results.Count == 0) return null;
            if (!(results[0] is Dictionary<string, object> first)) return null;
            var version = first.TryGetValue("version", out var v) ? v as string : null;
            var url = first.TryGetValue("trackViewUrl", out var u) ? u as string : null;
            if (string.IsNullOrWhiteSpace(version) || url == null || !url.StartsWith(AppStorePrefix, StringComparison.Ordinal)) return null;
            return new StoreRelease(version.Trim(), url);
        }
    }
}
