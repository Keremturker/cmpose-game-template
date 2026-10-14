using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.App.Settings
{
    public readonly struct SettingsRow
    {
        public SettingsRow(string titleKey, SettingsOption value)
        {
            TitleKey = titleKey;
            Value = value;
        }

        public string TitleKey { get; }
        public SettingsOption Value { get; }
    }
}
