using UnityEngine;

namespace Kturker.UnityBase.Core.About
{
    public sealed class UnityUrlOpener : IUrlOpener
    {
        public void Open(string url) => Application.OpenURL(url);
    }
}
