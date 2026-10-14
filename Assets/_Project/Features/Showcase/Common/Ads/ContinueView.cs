using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Showcase.Common.Ads
{
    // Prefab under Resources (SceneBuilder builds it); Spawn puts it into the game-over panel's action slot
    public sealed class ContinueView : MonoBehaviour, IContinueView
    {
        public const string ResourcePath = "ShowcaseAds/Continue";

        [SerializeField] private Button continueButton;
        [SerializeField] private LocalizedText label;
        [SerializeField] private LocalizedText hint;

        public event Action ContinueClicked;

        public static ContinueView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<ContinueView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        private void Awake()
        {
            continueButton.onClick.AddListener(OnContinue);
            Hide();
        }

        private void OnDestroy() => continueButton.onClick.RemoveListener(OnContinue);

        public void Show(bool viaAd)
        {
            label.SetKey(viaAd ? ShowcaseKeys.ContinueAd : ShowcaseKeys.Continue);
            continueButton.gameObject.SetActive(true);
            hint.gameObject.SetActive(false);
        }

        public void Hide()
        {
            continueButton.gameObject.SetActive(false);
            hint.gameObject.SetActive(false);
        }

        public void ShowHint(string key)
        {
            continueButton.gameObject.SetActive(false);
            hint.SetKey(key);
            hint.gameObject.SetActive(true);
        }

        private void OnContinue() => ContinueClicked?.Invoke();
    }
}
