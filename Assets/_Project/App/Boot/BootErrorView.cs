using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.App.Boot
{
    public sealed class BootErrorView : MonoBehaviour, IBootErrorView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private LocalizedText message;
        [SerializeField] private Button retryButton;
        private Action retry;

        private void Awake()
        {
            panel.SetActive(false);
            retryButton.onClick.AddListener(OnRetry);
        }

        private void OnDestroy() => retryButton.onClick.RemoveListener(OnRetry);

        public void Show(string messageKey, string detail, Action onRetry)
        {
            if (detail == null) message.SetKey(messageKey);
            else message.SetKey(messageKey, detail);
            retry = onRetry;
            panel.SetActive(true);
        }

        private void OnRetry()
        {
            panel.SetActive(false);
            var pending = retry;
            retry = null;
            pending?.Invoke();
        }
    }
}
