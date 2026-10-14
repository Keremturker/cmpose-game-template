using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Kturker.UnityBase.Core.Domain;
using UnityEngine;

namespace Kturker.UnityBase.Core.Save
{
    // One file per slot: {"version":N,"data":<JsonUtility JSON>}. A save is written to .tmp, the previous file
    // becomes .bak, then .tmp takes its place, so a crash leaves the old or the new save. A missing or unreadable
    // file falls back to .bak; a save from a newer version is never read (and not replaced by its backup).
    public sealed class FileSaveStore : ISaveStore
    {
        private static readonly Regex SlotName = new Regex("^[a-z0-9][a-z0-9_-]{0,63}$");
        private static readonly Regex Envelope = new Regex("^\\{\"version\":(-?\\d{1,9}),\"data\":(.*)\\}$", RegexOptions.Singleline);

        private readonly IFileSystem files;
        private readonly string root;
        private readonly ILog log;

        public FileSaveStore(IFileSystem files, string root, ILog log)
        {
            this.files = files;
            this.root = root;
            this.log = log;
        }

        private enum Outcome
        {
            Missing,
            Corrupt,
            TooNew,
            Read
        }

        public T Load<T>(SaveSchema<T> schema) where T : class
        {
            var main = PathOf(schema.Slot);
            var first = TryRead(main, schema, out var data);
            if (first == Outcome.Read) return data;
            if (first == Outcome.TooNew) return null;
            var backup = TryRead(main + ".bak", schema, out data);
            if (backup == Outcome.Read)
            {
                log.Warning($"Save '{schema.Slot}' could not be read; loaded the backup");
                return data;
            }
            if (first == Outcome.Corrupt || backup == Outcome.Corrupt) log.Warning($"Save '{schema.Slot}' is unreadable; starting fresh");
            return null;
        }

        public void Save<T>(SaveSchema<T> schema, T data) where T : class
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var main = PathOf(schema.Slot);
            var text = $"{{\"version\":{schema.Version.ToString(CultureInfo.InvariantCulture)},\"data\":{JsonUtility.ToJson(data)}}}";
            files.CreateDirectory(root);
            files.WriteAllText(main + ".tmp", text);
            if (files.Exists(main))
            {
                if (files.Exists(main + ".bak")) files.Delete(main + ".bak");
                files.Move(main, main + ".bak");
            }
            files.Move(main + ".tmp", main);
        }

        public void Delete(string slot)
        {
            var main = PathOf(slot);
            foreach (var path in new[] { main, main + ".bak", main + ".tmp" })
            {
                if (files.Exists(path)) files.Delete(path);
            }
        }

        private string PathOf(string slot)
        {
            if (slot == null || !SlotName.IsMatch(slot)) throw new ArgumentException($"invalid save slot '{slot}'", nameof(slot));
            return Path.Combine(root, slot + ".json");
        }

        private Outcome TryRead<T>(string path, SaveSchema<T> schema, out T data) where T : class
        {
            data = null;
            if (!files.Exists(path)) return Outcome.Missing;
            string text;
            try
            {
                text = files.ReadAllText(path);
            }
            catch (IOException e)
            {
                log.Warning($"Save '{schema.Slot}': {e.Message}");
                return Outcome.Corrupt;
            }
            var match = Envelope.Match(text ?? string.Empty);
            if (!match.Success || !int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version) || version < 1)
                return Outcome.Corrupt;
            if (version > schema.Version)
            {
                log.Error($"Save '{schema.Slot}' is version {version}, newer than this game's {schema.Version}; not loaded");
                return Outcome.TooNew;
            }
            var json = match.Groups[2].Value;
            while (version < schema.Version)
            {
                var step = schema.MigrationFrom(version);
                if (step == null)
                {
                    log.Error($"Save '{schema.Slot}': no migration from version {version}");
                    return Outcome.Corrupt;
                }
                try
                {
                    json = step.Migrate(json);
                }
                catch (Exception e)
                {
                    log.Error($"Save '{schema.Slot}': migration from {version} failed", e);
                    return Outcome.Corrupt;
                }
                version = step.To;
            }
            if (!JsonText.TryParse(json, out var parsed) || !(parsed is Dictionary<string, object>)) return Outcome.Corrupt;
            try
            {
                data = JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException)
            {
                return Outcome.Corrupt;
            }
            return data == null ? Outcome.Corrupt : Outcome.Read;
        }
    }
}
