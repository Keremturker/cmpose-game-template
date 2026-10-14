using NUnit.Framework;

namespace Kturker.UnityBase.Core.Leaderboards.Tests
{
    public sealed class GameServicesSectionTests
    {
        private FakeGameServices services;
        private GameServicesSection section;

        [SetUp]
        public void Create()
        {
            services = new FakeGameServices();
            section = new GameServicesSection(services);
        }

        [Test]
        public void The_title_is_the_game_services_key()
        {
            Assert.That(section.TitleKey, Is.EqualTo(LeaderboardsKeys.Title));
        }

        [Test]
        public void Unavailable_services_show_Not_set_up_and_a_press_does_nothing()
        {
            services.Available = false;

            Assert.That(section.Options.Count, Is.EqualTo(1));
            Assert.That(section.Options[0].LabelKey, Is.EqualTo(LeaderboardsKeys.NotSetUp));
            Assert.That(section.Selected, Is.Zero);
            section.Select(0);
            Assert.That(services.SignIns, Is.Zero);
        }

        [Test]
        public void A_signed_out_player_sees_Sign_in()
        {
            services.SignedIn = false;

            Assert.That(section.Options[0].LabelKey, Is.EqualTo(LeaderboardsKeys.SignIn));
        }

        [Test]
        public void A_signed_in_player_sees_Signed_in_and_a_press_does_nothing()
        {
            services.SignedIn = true;

            Assert.That(section.Options[0].LabelKey, Is.EqualTo(LeaderboardsKeys.SignedIn));
            section.Select(0);
            Assert.That(services.SignIns, Is.Zero);
        }

        [Test]
        public void A_press_on_Sign_in_asks_to_sign_in()
        {
            section.Select(0);
            section.Select(1);

            Assert.That(services.SignIns, Is.EqualTo(1));
        }

        [Test]
        public void Changed_follows_the_services()
        {
            var changes = 0;
            section.Changed += () => changes++;

            services.RaiseChanged();

            Assert.That(changes, Is.EqualTo(1));
        }
    }
}
