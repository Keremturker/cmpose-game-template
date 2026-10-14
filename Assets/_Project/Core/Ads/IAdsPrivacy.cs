using System;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Ads
{
    // UMP's privacy options: whether the law requires offering them here, and the form that changes consent
    public interface IAdsPrivacy
    {
        bool OptionsRequired { get; }

        // After the consent flow or the privacy form finished
        event Action Changed;

        UniTask ShowOptionsAsync();
    }
}
