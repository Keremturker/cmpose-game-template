using NUnit.Framework;

namespace Kturker.UnityBase.Core.Ads.Tests
{
    public sealed class PrivacySectionTests
    {
        [Test]
        public void The_row_shows_only_when_privacy_options_are_required()
        {
            var privacy = new FakeAdsPrivacy();
            var section = new PrivacySection(privacy);

            Assert.That(section.TitleKey, Is.EqualTo(AdsKeys.PrivacyTitle));
            Assert.That(section.Options, Is.Empty);
            privacy.OptionsRequired = true;
            Assert.That(section.Options.Count, Is.EqualTo(1));
            Assert.That(section.Options[0].LabelKey, Is.EqualTo(AdsKeys.PrivacyManage));
            Assert.That(section.Selected, Is.Zero);
        }

        [Test]
        public void A_press_opens_the_privacy_form_only_when_it_is_required()
        {
            var privacy = new FakeAdsPrivacy();
            var section = new PrivacySection(privacy);

            section.Select(0);
            privacy.OptionsRequired = true;
            section.Select(1);
            section.Select(0);

            Assert.That(privacy.Shown, Is.EqualTo(1));
        }

        [Test]
        public void Changed_follows_the_consent_flow()
        {
            var privacy = new FakeAdsPrivacy();
            var section = new PrivacySection(privacy);
            var changes = 0;
            section.Changed += () => changes++;

            privacy.RaiseChanged();

            Assert.That(changes, Is.EqualTo(1));
        }
    }
}
