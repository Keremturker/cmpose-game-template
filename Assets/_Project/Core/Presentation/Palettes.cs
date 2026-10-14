using System;
using UnityEngine;

namespace Kturker.UnityBase.Core.Presentation
{
    public static class Palettes
    {
        public const string Light = "Theme/Light";

        public static Palette Load(string resourcesPath)
        {
            var palette = Resources.Load<Palette>(resourcesPath);
            if (palette == null) throw new InvalidOperationException($"Resources/{resourcesPath}.asset is missing");
            return palette;
        }
    }
}
