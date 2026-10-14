using System;
using Kturker.UnityBase.Core.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.App.Settings
{
    // One of the settings panel's section rows: the title, and a button showing the selected option
    public sealed class SettingsRowView : MonoBehaviour
    {
        [SerializeField] private LocalizedText title;
        [SerializeField] private LocalizedText value;
        [SerializeField] private Button button;

        public event Action Clicked;

        public Button Button => button;

        private void Awake() => button.onClick.AddListener(OnClick);

        private void OnDestroy() => button.onClick.RemoveListener(OnClick);

        public void Show(SettingsRow row)
        {
            title.SetKey(row.TitleKey);
            if (row.Value.Literal != null) value.SetLiteral(row.Value.Literal);
            else value.SetKey(row.Value.LabelKey);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnClick() => Clicked?.Invoke();
    }
}
