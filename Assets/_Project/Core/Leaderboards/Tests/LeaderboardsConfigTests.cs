using NUnit.Framework;

namespace Kturker.UnityBase.Core.Leaderboards.Tests
{
    public sealed class LeaderboardsConfigTests
    {
        private static GameIds Real() => new GameIds("CgkIrealboard", new[]
        {
            new AchievementId(AchievementKeys.FirstCompletion, "CgkIrealfirst", 0),
            new AchievementId(AchievementKeys.TenCompletions, "CgkIrealten", 10)
        });

        [Test]
        public void Ids_are_set_on_both_platforms()
        {
            foreach (var ios in new[] { false, true })
            {
                var ids = LeaderboardsConfig.Ids(ios);
                Assert.That(ids.Leaderboard, Is.Not.Null.And.Not.Empty);
                foreach (var achievement in ids.Achievements)
                    Assert.That(achievement.Id, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void Placeholder_ids_are_detected_as_placeholders()
        {
            Assert.That(LeaderboardsConfig.IsPlaceholder("REPLACE_ME_high_score"), Is.True);
            Assert.That(LeaderboardsConfig.IsPlaceholder("cmpose.replace_me.high_score"), Is.True);
            var placeholders = new GameIds("REPLACE_ME_high_score", new[]
            {
                new AchievementId(AchievementKeys.FirstCompletion, "REPLACE_ME_first_completion", 0),
                new AchievementId(AchievementKeys.TenCompletions, "REPLACE_ME_ten_completions", 10)
            });
            Assert.That(LeaderboardsConfig.IsConfigured(placeholders), Is.False);
        }

        [Test]
        public void A_full_set_of_real_ids_is_configured()
        {
            Assert.That(LeaderboardsConfig.IsConfigured(Real()), Is.True);
        }

        [Test]
        public void One_placeholder_among_real_ids_keeps_the_service_off()
        {
            var androidStyle = new GameIds("CgkIrealboard", new[]
            {
                new AchievementId(AchievementKeys.FirstCompletion, "CgkIrealfirst", 0),
                new AchievementId(AchievementKeys.TenCompletions, "REPLACE_ME_ten_completions", 10)
            });
            var iosStyle = new GameIds("dev.example.board", new[]
            {
                new AchievementId(AchievementKeys.FirstCompletion, "cmpose.replace_me.first_completion", 0),
                new AchievementId(AchievementKeys.TenCompletions, "dev.example.ten", 10)
            });
            var noBoard = new GameIds("cmpose.replace_me.high_score", Real().Achievements);

            Assert.That(LeaderboardsConfig.IsConfigured(androidStyle), Is.False);
            Assert.That(LeaderboardsConfig.IsConfigured(iosStyle), Is.False);
            Assert.That(LeaderboardsConfig.IsConfigured(noBoard), Is.False);
        }

        [Test]
        public void Empty_ids_are_placeholders()
        {
            Assert.That(LeaderboardsConfig.IsPlaceholder(null), Is.True);
            Assert.That(LeaderboardsConfig.IsPlaceholder(""), Is.True);
            Assert.That(LeaderboardsConfig.IsPlaceholder("   "), Is.True);
            Assert.That(LeaderboardsConfig.IsPlaceholder("CgkIrealboard"), Is.False);
        }

        [Test]
        public void Ids_follow_the_platform()
        {
            var android = LeaderboardsConfig.Ids(false);
            var ios = LeaderboardsConfig.Ids(true);

            Assert.That(android.Leaderboard, Is.EqualTo(LeaderboardsConfig.LeaderboardAndroid));
            Assert.That(ios.Leaderboard, Is.EqualTo(LeaderboardsConfig.LeaderboardIos));
            Assert.That(android.Achievements.Count, Is.EqualTo(2));
            Assert.That(ios.Achievements.Count, Is.EqualTo(2));
        }

        [Test]
        public void An_achievement_is_found_by_its_key()
        {
            var ids = LeaderboardsConfig.Ids(false);

            Assert.That(ids.TryFind(AchievementKeys.TenCompletions, out var ten), Is.True);
            Assert.That(ten.Id, Is.EqualTo(LeaderboardsConfig.TenCompletionsAndroid));
            Assert.That(ten.Steps, Is.EqualTo(LeaderboardsConfig.TenCompletionsSteps));
            Assert.That(ids.TryFind(AchievementKeys.FirstCompletion, out var first), Is.True);
            Assert.That(first.Steps, Is.Zero);
            Assert.That(ids.TryFind("nope", out _), Is.False);
        }
    }
}
