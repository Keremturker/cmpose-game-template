using System;
using Kturker.UnityBase.Core.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // The game-over panel of a showcase game scene (SceneBuilder builds it); flag parts put their rows into ActionSlot
    public sealed class GameOverView : MonoBehaviour, IGameOverView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private LocalizedText score;
        [SerializeField] private LocalizedText best;
        [SerializeField] private GameObject newBest;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private RectTransform actionSlot;

        public event Action RetryClicked;
        public event Action MenuClicked;

        public Transform ActionSlot => actionSlot;
        public bool IsShown => panel.activeSelf;

        private void Awake()
        {
            panel.SetActive(false);
            retryButton.onClick.AddListener(OnRetry);
            menuButton.onClick.AddListener(OnMenu);
        }

        private void OnDestroy()
        {
            retryButton.onClick.RemoveListener(OnRetry);
            menuButton.onClick.RemoveListener(OnMenu);
        }

        public void Show(int scoreValue, int bestValue, bool isNewBest)
        {
            score.SetKey(ShowcaseKeys.Score, scoreValue);
            best.SetKey(ShowcaseKeys.Best, bestValue);
            newBest.SetActive(isNewBest);
            panel.SetActive(true);
        }

        public void Hide() => panel.SetActive(false);

        private void OnRetry() => RetryClicked?.Invoke();

        private void OnMenu() => MenuClicked?.Invoke();
    }
}
