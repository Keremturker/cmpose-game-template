using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Sample.Ads
{
    // Prefab under Resources (SceneBuilder builds it); Spawn puts it into the sample's action slot and binds it
    public sealed class BonusView : MonoBehaviour, IBonusView
    {
        public const string ResourcePath = "SampleAds/Bonus";

        [SerializeField] private Button bonusButton;
        [SerializeField] private LocalizedText hint;

        public event Action BonusClicked;

        public static BonusView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<BonusView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        private void Awake() => bonusButton.onClick.AddListener(OnBonus);

        private void OnDestroy() => bonusButton.onClick.RemoveListener(OnBonus);

        public void ShowHint(string key) => hint.SetKey(key);

        private void OnBonus() => BonusClicked?.Invoke();
    }
}
