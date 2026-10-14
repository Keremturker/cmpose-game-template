using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Update
{
    // The oldest version still supported, from the file at AppConfig.minVersionUrl:
    // {"minVersion":{"android":<versionCode>,"ios":"<CFBundleShortVersionString>"}}. Either platform may be missing.
    public sealed class MinVersions
    {
        public MinVersions(int? android, string ios)
        {
            Android = android;
            Ios = ios;
        }

        public int? Android { get; }
        public string Ios { get; }

        // Null for anything else (broken JSON, an HTML error page, a field of the wrong type): no forced update.
        // Unknown keys are ignored.
        public static MinVersions Parse(string json)
        {
            if (!JsonText.TryParse(json, out var root) || !(root is Dictionary<string, object> file)) return null;
            if (!file.TryGetValue("minVersion", out var section) || !(section is Dictionary<string, object> min)) return null;
            int? android = null;
            if (min.TryGetValue("android", out var code) && code != null)
            {
                if (!(code is double number) || number != Math.Floor(number) || number < int.MinValue || number > int.MaxValue) return null;
                android = (int)number;
            }
            string ios = null;
            if (min.TryGetValue("ios", out var name) && name != null)
            {
                if (!(name is string text)) return null;
                ios = text;
            }
            return new MinVersions(android, ios);
        }
    }
}
