using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Localization
{
    // Every table file of a language; empty when the language has none
    public interface IStringSource
    {
        IReadOnlyList<StringFile> Load(string language);
    }
}
