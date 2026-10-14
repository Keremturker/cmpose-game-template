using System;
using System.Collections.Generic;
using System.Linq;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Preferences;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;

namespace Kturker.UnityBase.Core.Language
{
    // multiLang flag: the Language row in Settings. The first launch takes the device language when supported,
    // English otherwise; a choice is stored in lang.code and wins from then on.
    public sealed class LanguageSelector : ISettingsSection
    {
        public const string PreferenceKey = "lang.code";

        public static readonly IReadOnlyList<(string Code, string Name)> Supported = new[]
        {
            ("en", "English"), ("tr", "Türkçe"), ("fr", "Français"), ("es", "Español"), ("it", "Italiano")
        };

        private static readonly SettingsOption[] Names = Supported.Select(l => SettingsOption.Text(l.Name)).ToArray();

        private readonly TableLocalizer localizer;
        private readonly IPreferences preferences;
        private readonly ISystemLanguage system;
        private readonly ILog log;

        public LanguageSelector(TableLocalizer localizer, IPreferences preferences, ISystemLanguage system, ILog log)
        {
            this.localizer = localizer;
            this.preferences = preferences;
            this.system = system;
            this.log = log;
        }

        public string TitleKey => "settings.language";
        public IReadOnlyList<SettingsOption> Options => Names;
        public int Selected => IndexOf(localizer.Language);

        public event Action Changed;

        // Called once when the root scope is built
        public void Apply()
        {
            var stored = preferences.GetString(PreferenceKey, null);
            Use(IndexOf(stored) >= 0 ? stored : FromSystem(system.Language));
        }

        public void Select(int index)
        {
            if (index < 0 || index >= Supported.Count) return;
            var code = Supported[index].Code;
            preferences.SetString(PreferenceKey, code);
            Use(code);
        }

        public static string FromSystem(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Italian: return "it";
                default: return TableLocalizer.DefaultLanguage;
            }
        }

        private void Use(string code)
        {
            if (!localizer.SetLanguage(code)) localizer.SetLanguage(TableLocalizer.DefaultLanguage);
            log.Marker($"CMPOSE_LANG code={localizer.Language}");
            Changed?.Invoke();
        }

        private static int IndexOf(string code)
        {
            for (var i = 0; i < Supported.Count; i++)
            {
                if (Supported[i].Code == code) return i;
            }
            return -1;
        }
    }
}
