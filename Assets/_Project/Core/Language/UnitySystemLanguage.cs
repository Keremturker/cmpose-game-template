using UnityEngine;

namespace Kturker.UnityBase.Core.Language
{
    public sealed class UnitySystemLanguage : ISystemLanguage
    {
        public SystemLanguage Language => Application.systemLanguage;
    }
}
