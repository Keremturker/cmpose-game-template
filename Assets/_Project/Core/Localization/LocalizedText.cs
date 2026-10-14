using System;
using Kturker.UnityBase.Core.Domain;
using TMPro;
using UnityEngine;

namespace Kturker.UnityBase.Core.Localization
{
    // Fills the TMP text on this object from a string key and refreshes when the language changes. An empty key
    // means code writes the text (SetLiteral, or the TMP text directly). UiBinder binds it; until then it keeps the
    // English text SceneBuilder wrote.
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        private TMP_Text label;
        private ILocalizer localizer;
        private object[] args = Array.Empty<object>();
        private string literal;

        public string Key => key;

        public void Bind(ILocalizer value)
        {
            if (localizer != value)
            {
                if (localizer != null) localizer.Changed -= Refresh;
                localizer = value;
                if (localizer != null) localizer.Changed += Refresh;
            }
            Refresh();
        }

        public void SetKey(string value, params object[] values)
        {
            key = value;
            args = values ?? Array.Empty<object>();
            literal = null;
            Refresh();
        }

        public void SetLiteral(string value)
        {
            literal = value;
            Refresh();
        }

        private void OnDestroy()
        {
            if (localizer != null) localizer.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (!this) return;
            if (label == null) label = GetComponent<TMP_Text>();
            if (literal != null) label.text = literal;
            else if (localizer != null && !string.IsNullOrEmpty(key)) label.text = localizer.Get(key, args);
        }
    }
}
