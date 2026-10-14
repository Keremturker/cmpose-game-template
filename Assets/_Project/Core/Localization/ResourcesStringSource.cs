using System.Collections.Generic;
using UnityEngine;

namespace Kturker.UnityBase.Core.Localization
{
    // Tables are TextAssets in any Resources/Strings/<language>/ folder: the core's, each feature module's and the
    // language flag's. Deleting a folder removes its strings without touching a reference.
    public sealed class ResourcesStringSource : IStringSource
    {
        public const string Root = "Strings";

        public IReadOnlyList<StringFile> Load(string language)
        {
            var assets = Resources.LoadAll<TextAsset>($"{Root}/{language}");
            var files = new List<StringFile>(assets.Length);
            foreach (var asset in assets) files.Add(new StringFile(asset.name, asset.text));
            return files;
        }
    }
}
