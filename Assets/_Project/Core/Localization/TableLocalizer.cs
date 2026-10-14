using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Localization
{
    // Flat JSON tables ({"key": "text"}), one or more files per language. Files merge in name order; a key in two
    // files keeps the first and logs an error. A missing key shows the key itself, warned once per key when asked
    // (debug builds). Starts in English; SetLanguage switches and raises Changed.
    public sealed class TableLocalizer : ILocalizer
    {
        public const string DefaultLanguage = "en";

        private readonly IStringSource source;
        private readonly ILog log;
        private readonly bool warnMissing;
        private readonly HashSet<string> warned = new HashSet<string>();
        private Dictionary<string, string> table = new Dictionary<string, string>();

        public TableLocalizer(IStringSource source, ILog log, bool warnMissing)
        {
            this.source = source;
            this.log = log;
            this.warnMissing = warnMissing;
            Language = DefaultLanguage;
            SetLanguage(DefaultLanguage);
        }

        public string Language { get; private set; }

        public event Action Changed;

        // False, and nothing changes, when the language has no table file
        public bool SetLanguage(string language)
        {
            var loaded = Load(language);
            if (loaded == null)
            {
                log.Warning($"No strings for language '{language}'");
                return false;
            }
            table = loaded;
            Language = language;
            warned.Clear();
            Changed?.Invoke();
            return true;
        }

        public string Get(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (!table.TryGetValue(key, out var text))
            {
                if (warnMissing && warned.Add(key)) log.Warning($"Missing string '{key}' for '{Language}'");
                return key;
            }
            if (args == null || args.Length == 0) return text;
            try
            {
                return string.Format(CultureInfo.InvariantCulture, text, args);
            }
            catch (FormatException)
            {
                log.Warning($"String '{key}' for '{Language}' does not fit {args.Length} argument(s)");
                return text;
            }
        }

        private Dictionary<string, string> Load(string language)
        {
            if (string.IsNullOrEmpty(language)) return null;
            var files = source.Load(language);
            if (files == null || files.Count == 0) return null;
            var merged = new Dictionary<string, string>();
            foreach (var file in files.OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                if (!JsonText.TryParse(file.Json, out var parsed) || !(parsed is Dictionary<string, object> entries))
                {
                    log.Error($"Strings/{language}/{file.Name}: not a JSON object");
                    continue;
                }
                foreach (var entry in entries)
                {
                    if (!(entry.Value is string value))
                    {
                        log.Error($"Strings/{language}/{file.Name}: '{entry.Key}' is not a string");
                        continue;
                    }
                    if (merged.ContainsKey(entry.Key))
                    {
                        log.Error($"Strings/{language}/{file.Name}: '{entry.Key}' is already defined");
                        continue;
                    }
                    merged.Add(entry.Key, value);
                }
            }
            return merged;
        }
    }
}
