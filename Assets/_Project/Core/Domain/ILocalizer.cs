using System;

namespace Kturker.UnityBase.Core.Domain
{
    // Text for a string key in the current language; Changed fires after the language changes
    public interface ILocalizer
    {
        string Language { get; }
        string Get(string key, params object[] args);
        event Action Changed;
    }
}
