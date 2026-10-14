using System;
using Kturker.UnityBase.Core.Preferences;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // The best finished score, kept in IPreferences (no flag needed)
    public sealed class BestScore
    {
        public const string Key = "showcase.best";

        private readonly IPreferences preferences;

        public BestScore(IPreferences preferences) => this.preferences = preferences;

        public int Value => Math.Max(0, preferences.GetInt(Key, 0));

        // True when the score beats the stored best, which it then replaces
        public bool Submit(int score)
        {
            if (score <= Value) return false;
            preferences.SetInt(Key, score);
            preferences.Save();
            return true;
        }
    }
}
