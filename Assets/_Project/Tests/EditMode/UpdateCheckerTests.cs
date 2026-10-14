using System;
using System.Globalization;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UpdateCheckerTests
    {
        private const long Now = 1_800_000_000_000L;
        private const long Hour = 60L * 60 * 1000;
        private const string Url = "https://example.com/min.json";
        private const string Lookup = "https://itunes.apple.com/lookup?bundleId=com.example.game";

        private FakePreferences prefs;
        private FakeHttpText http;
        private FakePlatformUpdater platform;
        private FakeClock clock;
        private FakeLog log;

        [SetUp]
        public void Create()
        {
            prefs = new FakePreferences();
            http = new FakeHttpText();
            platform = new FakePlatformUpdater { Platform = StorePlatform.Android, CurrentVersion = new AppVersion("1.0.0", 5) };
            clock = new FakeClock { NowMillis = Now };
            log = new FakeLog();
        }

        // The fakes complete synchronously, so the task has already finished
        private UpdateDecision Check(string url = Url) =>
            new UpdateChecker(prefs, http, platform, clock, new UpdateConfig(url, "com.example.game"), log)
                .CheckAsync(default).GetAwaiter().GetResult();

        [Test]
        public void No_request_is_sent_when_no_check_is_due()
        {
            prefs.SetString(UpdateChecker.LastCheckKey, (Now - Hour).ToString(CultureInfo.InvariantCulture));

            Assert.That(Check(), Is.Null);
            Assert.That(http.Requested, Is.Empty);
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_UPDATE not-due"));
        }

        [Test]
        public void A_due_check_records_the_decision_and_the_time()
        {
            http.Responses[Url] = "{\"minVersion\":{\"android\":9}}";

            Assert.That(Check(), Is.EqualTo(UpdateDecision.Required(null)));
            Assert.That(prefs.GetString(UpdateChecker.LastCheckKey, ""), Is.EqualTo("1800000000000"));
            Assert.That(prefs.GetBool(UpdateChecker.LastRequiredKey, false), Is.True);
            Assert.That(prefs.Saves, Is.EqualTo(1));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_UPDATE decision=Required"));

            // A required update is checked again on every launch
            Assert.That(Check(), Is.EqualTo(UpdateDecision.Required(null)));
            Assert.That(http.Requested.Count, Is.EqualTo(2));
        }

        [Test]
        public void A_network_failure_is_no_update_and_still_records_the_time()
        {
            Assert.That(Check(), Is.EqualTo(UpdateDecision.None));
            Assert.That(prefs.GetString(UpdateChecker.LastCheckKey, ""), Is.EqualTo("1800000000000"));
            Assert.That(prefs.GetBool(UpdateChecker.LastRequiredKey, true), Is.False);

            clock.NowMillis = Now + 25 * Hour;
            http.Failure = new InvalidOperationException("timeout");

            Assert.That(Check(), Is.EqualTo(UpdateDecision.None));
            Assert.That(log.Lines, Does.Contain("WARNING Update check request failed: InvalidOperationException"));
            Assert.That(http.Requested.Count, Is.EqualTo(2));
            Assert.That(prefs.GetString(UpdateChecker.LastCheckKey, ""), Is.EqualTo((Now + 25 * Hour).ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public void Ios_asks_the_app_store_and_android_does_not()
        {
            Assert.That(Check(url: ""), Is.EqualTo(UpdateDecision.None));
            Assert.That(http.Requested, Is.Empty);

            prefs.Values.Clear();
            platform.Platform = StorePlatform.Ios;
            platform.CurrentVersion = new AppVersion("1.0.0", null);
            http.Responses[Lookup] = "{\"resultCount\":1,\"results\":[{\"version\":\"1.1.0\",\"trackViewUrl\":\"https://apps.apple.com/app/id1\"}]}";

            Assert.That(Check(url: ""), Is.EqualTo(UpdateDecision.Available("https://apps.apple.com/app/id1")));
            Assert.That(http.Requested, Is.EqualTo(new[] { Lookup }));
            Assert.That(log.Lines, Does.Contain("MARKER CMPOSE_UPDATE decision=Available"));
        }

        [Test]
        public void A_corrupt_stored_last_check_counts_as_never_checked()
        {
            prefs.SetString(UpdateChecker.LastCheckKey, "yesterday");
            Assert.That(Check(), Is.EqualTo(UpdateDecision.None));

            prefs.SetInt(UpdateChecker.LastCheckKey, 42);
            Assert.That(Check(), Is.EqualTo(UpdateDecision.None));

            prefs.SetString(UpdateChecker.LastCheckKey, (Now + 2 * Hour).ToString(CultureInfo.InvariantCulture));
            Assert.That(Check(), Is.EqualTo(UpdateDecision.None));

            Assert.That(http.Requested.Count, Is.EqualTo(3));
        }
    }
}
