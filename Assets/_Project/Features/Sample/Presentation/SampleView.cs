using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Sample.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Sample.Presentation
{
    // Prefab under Resources (SceneBuilder builds it); SampleInstaller spawns it into the Game scene's ModuleSlot
    public sealed class SampleView : MonoBehaviour, ISampleView
    {
        public const string ResourcePath = "Sample/SampleView";

        [SerializeField] private string label = "Sample";
        [SerializeField] private LocalizedText text;
        [SerializeField] private LocalizedText doneText;
        [SerializeField] private LocalizedText completedText;
        [SerializeField] private Button completeButton;
        [SerializeField] private Button backButton;
        // Empty area above the greeting; feature flags put their sample cards here at runtime
        [SerializeField] private RectTransform extraSlot;
        // Empty area below Back; feature flags put their sample buttons here at runtime
        [SerializeField] private RectTransform actionSlot;
        // Empty area between the extra slot and the completion count; feature flags put their sample buttons here
        [SerializeField] private RectTransform scoreSlot;

        public event Action BackClicked;
        public event Action CompleteClicked;

        public string Label => label;
        public Button BackButton => backButton;
        public Button CompleteButton => completeButton;
        public RectTransform ExtraSlot => extraSlot;
        public RectTransform ActionSlot => actionSlot;
        public RectTransform ScoreSlot => scoreSlot;

        public static SampleView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<SampleView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        private void Awake()
        {
            backButton.onClick.AddListener(OnBack);
            completeButton.onClick.AddListener(OnComplete);
        }

        private void OnDestroy()
        {
            backButton.onClick.RemoveListener(OnBack);
            completeButton.onClick.RemoveListener(OnComplete);
        }

        public void ShowGreeting(string value) => text.SetKey(SampleKeys.Greeting, value);

        public void ShowDone() => doneText.SetKey(SampleKeys.Done);

        public void ShowCompleted(int count) => completedText.SetKey(SampleKeys.Completed, count);

        private void OnBack() => BackClicked?.Invoke();

        private void OnComplete() => CompleteClicked?.Invoke();
    }
}
