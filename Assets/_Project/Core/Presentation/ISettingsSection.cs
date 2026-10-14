using System;
using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Presentation
{
    // A row in the settings panel that a feature flag contributes through DI (register it as ISettingsSection);
    // the row shows the selected option and a press selects the next one. A section with no options is hidden.
    // Changed fires after the selection or the options change.
    public interface ISettingsSection
    {
        string TitleKey { get; }
        IReadOnlyList<SettingsOption> Options { get; }
        int Selected { get; }
        void Select(int index);
        event Action Changed;
    }
}
