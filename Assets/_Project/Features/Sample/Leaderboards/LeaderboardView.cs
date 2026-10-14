using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Sample.Leaderboards
{
    // Prefab under Resources (SceneBuilder builds it); Spawn puts it into the sample's score slot and binds it
    public sealed class LeaderboardView : MonoBehaviour, ILeaderboardView
    {
        public const string ResourcePath = "SampleLeaderboards/Scores";

        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private LocalizedText hint;

        public event Action LeaderboardClicked;
        public event Action AchievementsClicked;

        public static LeaderboardView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<LeaderboardView>(ResourcePath);
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
            hint.gameObject.SetActive(true);
            hint.SetKey(key);
        }

        public void HideHint() => hint.gameObject.SetActive(false);

        private void OnLeaderboard() => LeaderboardClicked?.Invoke();

        private void OnAchievements() => AchievementsClicked?.Invoke();
    }
}
