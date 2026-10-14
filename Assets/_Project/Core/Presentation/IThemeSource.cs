using System;

namespace Kturker.UnityBase.Core.Presentation
{
    // The palette in use; Changed fires after it changes
    public interface IThemeSource
    {
        Palette Current { get; }
        event Action Changed;
    }
}
