using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Core.Ads
{
    // The "Privacy choices" row in Settings: shown only when UMP says the privacy options are required here (a section
    // without options is hidden); a press opens UMP's privacy options form
    public sealed class PrivacySection : ISettingsSection
    {
        private static readonly SettingsOption[] Manage = { SettingsOption.Key(AdsKeys.PrivacyManage) };

        private readonly IAdsPrivacy privacy;

        public PrivacySection(IAdsPrivacy privacy)
        {
            this.privacy = privacy;
            privacy.Changed += OnChanged;
        }

        public string TitleKey => AdsKeys.PrivacyTitle;
        public IReadOnlyList<SettingsOption> Options => privacy.OptionsRequired ? Manage : Array.Empty<SettingsOption>();
        public int Selected => 0;

        public event Action Changed;

        public void Select(int index)
        {
            if (index != 0 || !privacy.OptionsRequired) return;
            privacy.ShowOptionsAsync().Forget();
        }

        private void OnChanged() => Changed?.Invoke();
    }
}
