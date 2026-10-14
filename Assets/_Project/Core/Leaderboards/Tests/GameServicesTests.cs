using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Leaderboards.Tests
{
    public sealed class GameServicesTests
    {
        private const string First = AchievementKeys.FirstCompletion;
        private const string Ten = AchievementKeys.TenCompletions;

        private FakeGameServicesGateway gateway;
        private FakePreferences prefs;
        private FakeLog log;

        [SetUp]
        public void Create()
        {
            gateway = new FakeGameServicesGateway();
            prefs = new FakePreferences();
            log = new FakeLog();
        }

        private static CancellationToken Token => CancellationToken.None;

        private GameServices Make(bool configured = true, bool warn = false) =>
            new GameServices(gateway, prefs, log, configured, warn);

        // A service that finished the silent sign-in with the given answer
        private GameServices Started(bool signedIn)
        {
            gateway.SilentSignIn = signedIn;
            var services = Make();
            services.Start();
            return services;
        }

        private static T Run<T>(UniTask<T> task) => task.GetAwaiter().GetResult();

        private IEnumerable<string> Markers => log.Lines.Where(l => l.StartsWith("MARKER CMPOSE_"));

        [Test]
        public void Unavailable_services_never_touch_the_gateway()
        {
            var services = Make(configured: false);
            services.Start();

            Assert.That(services.IsAvailable, Is.False);
            Assert.That(Run(services.SignInAsync(Token)), Is.EqualTo(GameServicesResult.Unavailable));
            Assert.That(Run(services.SubmitScoreAsync(10, Token)), Is.EqualTo(GameServicesResult.Unavailable));
            Assert.That(Run(services.UnlockAchievementAsync(First, Token)), Is.EqualTo(GameServicesResult.Unavailable));
            Assert.That(Run(services.IncrementAchievementAsync(Ten, 1, Token)), Is.EqualTo(GameServicesResult.Unavailable));
            Assert.That(Run(services.ShowLeaderboardAsync(Token)), Is.EqualTo(GameServicesResult.Unavailable));
            Assert.That(Run(services.ShowAchievementsAsync(Token)), Is.EqualTo(GameServicesResult.Unavailable));
            Assert.That(gateway.Calls, Is.Empty);
            Assert.That(prefs.Values, Is.Empty);
            Assert.That(Markers, Is.EqualTo(new[]
            {
                "MARKER CMPOSE_GAMES available=false signedIn=false",
                "MARKER CMPOSE_SCORE result=Unavailable",
                "MARKER CMPOSE_ACHIEVEMENT key=firstCompletion result=Unavailable",
                "MARKER CMPOSE_ACHIEVEMENT key=tenCompletions result=Unavailable"
            }));
        }

        [Test]
        public void Release_builds_with_placeholder_ids_warn_once_and_others_stay_quiet()
        {
            Make(configured: false, warn: true).Start();
            Make(configured: false, warn: false).Start();

            Assert.That(log.Lines.Count(l => l == "WARNING " + GameServices.PlaceholderWarning), Is.EqualTo(1));
        }

        [Test]
        public void Start_signs_in_silently_and_never_shows_a_screen()
        {
            var services = Started(true);

            Assert.That(services.IsAvailable, Is.True);
            Assert.That(services.IsSignedIn, Is.True);
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start" }));
            Assert.That(Markers, Is.EqualTo(new[] { "MARKER CMPOSE_GAMES available=true signedIn=true" }));
        }

        [Test]
        public void A_failed_silent_sign_in_leaves_the_player_signed_out()
        {
            var services = Started(false);

            Assert.That(services.IsSignedIn, Is.False);
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start" }));
            Assert.That(Markers, Is.EqualTo(new[] { "MARKER CMPOSE_GAMES available=true signedIn=false" }));
        }

        [Test]
        public void Sign_in_on_request_shows_the_screen_and_raises_Changed()
        {
            var services = Started(false);
            var changes = 0;
            services.Changed += () => changes++;

            Assert.That(Run(services.SignInAsync(Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(services.SignInAsync(Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(services.IsSignedIn, Is.True);
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "signIn" }));
        }

        [Test]
        public void A_declined_sign_in_is_NotSignedIn_and_can_be_asked_again()
        {
            var services = Started(false);
            gateway.InteractiveSignIn = false;

            Assert.That(Run(services.SignInAsync(Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(Run(services.SignInAsync(Token)), Is.EqualTo(GameServicesResult.NotSignedIn));

            Assert.That(services.IsSignedIn, Is.False);
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "signIn", "signIn" }));
        }

        [Test]
        public void A_score_while_signed_out_is_kept_and_sent_at_sign_in()
        {
            var services = Started(false);

            Assert.That(Run(services.SubmitScoreAsync(50, Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(Run(services.SubmitScoreAsync(20, Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start" }));

            Assert.That(Run(services.SignInAsync(Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "signIn", "submit 50" }));
            Assert.That(Run(services.SubmitScoreAsync(50, Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(gateway.Calls.Count, Is.EqualTo(3));
            Assert.That(Markers.Where(m => m.Contains("CMPOSE_SCORE")), Is.EqualTo(new[]
            {
                "MARKER CMPOSE_SCORE result=NotSignedIn",
                "MARKER CMPOSE_SCORE result=NotSignedIn",
                "MARKER CMPOSE_SCORE result=Ok"
            }));
        }

        [Test]
        public void A_lower_score_never_overwrites_a_better_one()
        {
            var services = Started(true);

            Assert.That(Run(services.SubmitScoreAsync(80, Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(services.SubmitScoreAsync(30, Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(services.SubmitScoreAsync(90, Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "submit 80", "submit 90" }));
        }

        [Test]
        public void A_failed_submit_keeps_the_value_and_the_next_call_retries()
        {
            var services = Started(true);
            gateway.Succeeds = false;

            Assert.That(Run(services.SubmitScoreAsync(70, Token)), Is.EqualTo(GameServicesResult.Failed));
            gateway.Succeeds = true;
            Assert.That(Run(services.SubmitScoreAsync(10, Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "submit 70", "submit 70" }));
        }

        [Test]
        public void Pending_achievements_are_sent_once_at_sign_in()
        {
            var services = Started(false);

            Assert.That(Run(services.UnlockAchievementAsync(First, Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(Run(services.IncrementAchievementAsync(Ten, 3, Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(Run(services.IncrementAchievementAsync(Ten, 2, Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start" }));

            Run(services.SignInAsync(Token));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "signIn", "unlock firstCompletion", "increment tenCompletions 5" }));

            Run(services.SignInAsync(Token));
            Assert.That(Run(services.UnlockAchievementAsync(First, Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(gateway.Calls.Count, Is.EqualTo(4));
            Assert.That(Markers.Where(m => m.Contains("CMPOSE_ACHIEVEMENT")).Last(),
                Is.EqualTo("MARKER CMPOSE_ACHIEVEMENT key=firstCompletion result=Ok"));
        }

        [Test]
        public void An_unlock_is_never_sent_twice()
        {
            var services = Started(true);

            Assert.That(Run(services.UnlockAchievementAsync(First, Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(services.UnlockAchievementAsync(First, Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "unlock firstCompletion" }));
        }

        [Test]
        public void A_failed_increment_keeps_its_steps()
        {
            var services = Started(true);
            gateway.Succeeds = false;

            Assert.That(Run(services.IncrementAchievementAsync(Ten, 2, Token)), Is.EqualTo(GameServicesResult.Failed));
            gateway.Succeeds = true;
            Assert.That(Run(services.IncrementAchievementAsync(Ten, 1, Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "increment tenCompletions 2", "increment tenCompletions 3" }));
        }

        [Test]
        public void Steps_of_zero_or_less_and_empty_keys_are_refused()
        {
            var services = Started(true);

            Assert.That(Run(services.IncrementAchievementAsync(Ten, 0, Token)), Is.EqualTo(GameServicesResult.Failed));
            Assert.That(Run(services.IncrementAchievementAsync(Ten, -1, Token)), Is.EqualTo(GameServicesResult.Failed));
            Assert.That(Run(services.UnlockAchievementAsync("", Token)), Is.EqualTo(GameServicesResult.Failed));
            Assert.That(Run(services.UnlockAchievementAsync(null, Token)), Is.EqualTo(GameServicesResult.Failed));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start" }));
            Assert.That(prefs.Values.Keys.Where(k => k.StartsWith("gameServices.steps") || k.StartsWith("gameServices.unlocks")), Is.Empty);
        }

        [Test]
        public void Scores_sent_while_a_send_runs_follow_in_the_same_round()
        {
            var services = Started(true);
            var held = new UniTaskCompletionSource<bool>();
            gateway.PendingSubmit = held;

            var first = services.SubmitScoreAsync(10, Token);
            var second = services.SubmitScoreAsync(20, Token);
            held.TrySetResult(true);

            Assert.That(Run(first), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(second), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "submit 10", "submit 20" }));
        }

        [Test]
        public void A_second_screen_request_while_one_is_open_is_refused()
        {
            var services = Started(true);
            var held = new UniTaskCompletionSource<bool>();
            gateway.PendingScreen = held;

            var first = services.ShowLeaderboardAsync(Token);
            Assert.That(Run(services.ShowAchievementsAsync(Token)), Is.EqualTo(GameServicesResult.Failed));
            held.TrySetResult(true);

            Assert.That(Run(first), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(services.ShowAchievementsAsync(Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "showLeaderboard", "showAchievements" }));
        }

        [Test]
        public void A_screen_asks_to_sign_in_first_and_stops_when_the_player_declines()
        {
            var services = Started(false);
            gateway.InteractiveSignIn = false;

            Assert.That(Run(services.ShowLeaderboardAsync(Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            Assert.That(Run(services.ShowAchievementsAsync(Token)), Is.EqualTo(GameServicesResult.NotSignedIn));
            gateway.InteractiveSignIn = true;
            Assert.That(Run(services.ShowLeaderboardAsync(Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "signIn", "signIn", "signIn", "showLeaderboard" }));
        }

        [Test]
        public void A_screen_for_a_signed_in_player_just_opens()
        {
            var services = Started(true);

            Assert.That(Run(services.ShowLeaderboardAsync(Token)), Is.EqualTo(GameServicesResult.Ok));
            Assert.That(Run(services.ShowAchievementsAsync(Token)), Is.EqualTo(GameServicesResult.Ok));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "showLeaderboard", "showAchievements" }));
        }

        [Test]
        public void A_screen_that_cannot_open_is_failed_and_frees_the_slot()
        {
            var services = Started(true);
            gateway.ScreenOpens = false;

            Assert.That(Run(services.ShowLeaderboardAsync(Token)), Is.EqualTo(GameServicesResult.Failed));
            gateway.ScreenOpens = true;
            Assert.That(Run(services.ShowLeaderboardAsync(Token)), Is.EqualTo(GameServicesResult.Ok));
        }

        [Test]
        public void Pending_state_survives_a_restart()
        {
            var first = Started(false);
            Run(first.SubmitScoreAsync(40, Token));
            Run(first.UnlockAchievementAsync(First, Token));
            Run(first.IncrementAchievementAsync(Ten, 4, Token));

            gateway = new FakeGameServicesGateway { SilentSignIn = true };
            Make().Start();

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start", "submit 40", "unlock firstCompletion", "increment tenCompletions 4" }));
        }

        [Test]
        public void A_cancelled_token_changes_nothing()
        {
            var services = Started(true);
            var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            Assert.Throws<OperationCanceledException>(() => Run(services.SubmitScoreAsync(5, cancelled.Token)));
            Assert.Throws<OperationCanceledException>(() => Run(services.UnlockAchievementAsync(First, cancelled.Token)));
            Assert.Throws<OperationCanceledException>(() => Run(services.ShowLeaderboardAsync(cancelled.Token)));

            Assert.That(gateway.Calls, Is.EqualTo(new[] { "start" }));
            Assert.That(prefs.Values, Is.Empty);
        }
    }
}
