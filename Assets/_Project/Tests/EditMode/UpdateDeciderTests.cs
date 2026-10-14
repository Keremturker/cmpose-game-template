using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UpdateDeciderTests
    {
        private const string StoreUrl = "https://apps.apple.com/app/id1";

        private static UpdateDecision IosAgainstStore(string current, string store) =>
            UpdateDecider.Decide(new AppVersion(current, null), null, new StoreRelease(store, StoreUrl), StorePlatform.Ios);

        [Test]
        public void An_android_code_below_the_minimum_is_required_and_an_equal_one_is_not()
        {
            Assert.That(UpdateDecider.Decide(new AppVersion("1.3.0", 11), new MinVersions(12, null), null, StorePlatform.Android),
                Is.EqualTo(UpdateDecision.Required(null)));
            Assert.That(UpdateDecider.Decide(new AppVersion("1.3.0", 12), new MinVersions(12, null), null, StorePlatform.Android),
                Is.EqualTo(UpdateDecision.None));
        }

        [Test]
        public void An_ios_name_below_the_minimum_is_required_with_the_store_url()
        {
            var decision = UpdateDecider.Decide(new AppVersion("1.3.0", null), new MinVersions(null, "1.4.0"),
                new StoreRelease("1.4.0", StoreUrl), StorePlatform.Ios);

            Assert.That(decision, Is.EqualTo(UpdateDecision.Required(StoreUrl)));
            Assert.That(UpdateDecider.Decide(new AppVersion("1.4", null), new MinVersions(null, "1.4.0"), null, StorePlatform.Ios),
                Is.EqualTo(UpdateDecision.None));
        }

        [Test]
        public void A_newer_store_version_is_available_and_the_same_one_is_not()
        {
            Assert.That(IosAgainstStore("1.9", "1.10"), Is.EqualTo(UpdateDecision.Available(StoreUrl)));
            Assert.That(IosAgainstStore("1.4.0", "1.4.0"), Is.EqualTo(UpdateDecision.None));
            Assert.That(IosAgainstStore("1.10", "1.9"), Is.EqualTo(UpdateDecision.None));
        }

        [Test]
        public void Nothing_known_is_no_update_and_an_unknown_version_never_forces()
        {
            Assert.That(UpdateDecider.Decide(new AppVersion("1.3.0", 11), null, null, StorePlatform.Ios), Is.EqualTo(UpdateDecision.None));
            Assert.That(UpdateDecider.Decide(new AppVersion("1.3.0", 11), null, null, StorePlatform.Android), Is.EqualTo(UpdateDecision.None));
            Assert.That(UpdateDecider.Decide(new AppVersion("1.3.0", null), new MinVersions(99, "9.0"), null, StorePlatform.Android),
                Is.EqualTo(UpdateDecision.None));
            Assert.That(UpdateDecider.Decide(new AppVersion("", null), new MinVersions(null, "9.0"), null, StorePlatform.Ios),
                Is.EqualTo(UpdateDecision.None));
        }

        [Test]
        public void Required_wins_over_available()
        {
            var decision = UpdateDecider.Decide(new AppVersion("1.0.0", null), new MinVersions(null, "1.2.0"),
                new StoreRelease("1.3.0", StoreUrl), StorePlatform.Ios);

            Assert.That(decision, Is.EqualTo(UpdateDecision.Required(StoreUrl)));
        }
    }
}
