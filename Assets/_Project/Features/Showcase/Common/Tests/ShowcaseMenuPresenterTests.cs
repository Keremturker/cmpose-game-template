using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Common.Tests
{
    public sealed class ShowcaseMenuPresenterTests
    {
        [Test]
        public void The_menu_shows_the_stored_best()
        {
            var preferences = new FakePreferences();
            preferences.SetInt(BestScore.Key, 77);
            var view = new FakeShowcaseMenuView();

            new ShowcaseMenuPresenter(view, new BestScore(preferences)).Start();

            Assert.That(view.Best, Is.EqualTo(77));
        }
    }
}
