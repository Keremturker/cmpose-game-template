using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.App.Game
{
    // "Start here" panel over the Sample feature in the blank shell
    public sealed class GuideView : MonoBehaviour, IGuideView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button gotItButton;

        public event Action DismissClicked;

        private void Awake()
        {
            panel.SetActive(false);
            gotItButton.onClick.AddListener(OnDismiss);
        }

        private void OnDestroy() => gotItButton.onClick.RemoveListener(OnDismiss);

        public void Show() => panel.SetActive(true);

        public void Hide() => panel.SetActive(false);

        private void OnDismiss() => DismissClicked?.Invoke();
    }
}
