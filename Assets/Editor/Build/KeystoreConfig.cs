using System;
using System.Collections.Generic;
using System.IO;

namespace Kturker.UnityBase.Editor.Build
{
    // keystore.properties: key=value lines, '#' or '!' comments, the first '=' splits, keys and values trimmed.
    // storeFile is absolute or relative to the project folder. Messages never contain a password.
    public sealed class KeystoreConfig
    {
        private static readonly string[] Required = { "storeFile", "storePassword", "keyAlias", "keyPassword" };

        private KeystoreConfig(string storeFile, string storePassword, string keyAlias, string keyPassword)
        {
            StoreFile = storeFile;
            StorePassword = storePassword;
            KeyAlias = keyAlias;
            KeyPassword = keyPassword;
        }

        public string StoreFile { get; }
        public string StorePassword { get; }
        public string KeyAlias { get; }
        public string KeyPassword { get; }

        public static KeystoreConfig Parse(string text, string projectRoot, Func<string, bool> fileExists)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == '!') continue;
                var equals = line.IndexOf('=');
                if (equals < 0) throw new BuildInputException($"{AndroidSigning.FileName}: line {i + 1} has no '='");
                values[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
            }
            foreach (var key in Required)
            {
                if (!values.TryGetValue(key, out var value)) throw new BuildInputException($"{AndroidSigning.FileName}: missing {key}");
                if (value.Length == 0) throw new BuildInputException($"{AndroidSigning.FileName}: empty {key}");
            }
            var store = values["storeFile"];
            var path = Path.GetFullPath(Path.IsPathRooted(store) ? store : Path.Combine(projectRoot, store));
            if (!fileExists(path)) throw new BuildInputException($"{AndroidSigning.FileName}: storeFile not found: {store}");
            return new KeystoreConfig(path, values["storePassword"], values["keyAlias"], values["keyPassword"]);
        }
    }
}
