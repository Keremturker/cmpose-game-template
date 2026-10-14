using System;

namespace Kturker.UnityBase.Core.Navigation
{
    public interface IBootErrorView
    {
        // messageKey is a string key; detail fills its {0} (the failed step's name), null when it has none
        void Show(string messageKey, string detail, Action onRetry);
    }
}
