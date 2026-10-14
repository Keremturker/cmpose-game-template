using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Showcase.Common.Leaderboards
{
    // A row in the menu's extra slot: Leaderboard and Achievements buttons and a hint line (SceneBuilder builds it)
    public sealed class ShowcaseScoresView : MonoBehaviour, IShowcaseScoresView
    {
        public const string ResourcePath = "ShowcaseLeaderboards/Scores";

        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private LocalizedText hint;

        public event Action LeaderboardClicked;
        public event Action AchievementsClicked;

        public static ShowcaseScoresView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<ShowcaseScoresView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        private void Awake()
        {
            leaderboardButton.onClick.AddListener(OnLeaderboard);
            achievementsButton.onClick.AddListener(OnAchievements);
            hint.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            leaderboardButton.onClick.RemoveListener(OnLeaderboard);
            achievementsButton.onClick.RemoveListener(OnAchievements);
        }

        public void ShowHint(string key)
        {
            hint.SetKey(key);
            hint.gameObject.SetActive(true);
        }

        public void HideHint() => hint.gameObject.SetActive(false);

        private void OnLeaderboard() => LeaderboardClicked?.Invoke();

        private void OnAchievements() => AchievementsClicked?.Invoke();
    }
}
