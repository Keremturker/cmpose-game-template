namespace Kturker.UnityBase.Core.Presentation
{
    // A choice in a settings section: a string key, or a literal shown as is (language names)
    public readonly struct SettingsOption
    {
        private SettingsOption(string labelKey, string literal)
        {
            LabelKey = labelKey;
            Literal = literal;
        }

        public string LabelKey { get; }
        public string Literal { get; }

        public static SettingsOption Key(string labelKey) => new SettingsOption(labelKey, null);

        public static SettingsOption Text(string literal) => new SettingsOption(null, literal);
    }
}
