using Kturker.UnityBase.App.About;
using Kturker.UnityBase.Core.About;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class AboutPresenterTests
    {
        [Test]
        public void Open_shows_the_version_once_and_hides_blank_legal_links()
        {
            var view = new FakeAboutView();
            var presenter = new AboutPresenter(view, new AboutInfo("1.0.0", "", "https://example.com/terms", "MIT"),
                new FakeUrlOpener(), new FakeUiFeedback());
            presenter.Start();

            presenter.Open();
            presenter.Open();

            Assert.That(view.Shown, Is.EqualTo(1));
            Assert.That(view.Version, Is.EqualTo("v1.0.0"));
            Assert.That(view.PrivacyVisible, Is.False);
            Assert.That(view.TermsVisible, Is.True);
            Assert.That(view.Licenses, Is.EqualTo("MIT"));
        }

        [Test]
        public void Links_open_their_url_and_close_hides_with_feedback()
        {
            var view = new FakeAboutView();
            var urls = new FakeUrlOpener();
            var feedback = new FakeUiFeedback();
            var presenter = new AboutPresenter(view, new AboutInfo("1.0.0", "https://example.com/privacy", "https://example.com/terms", ""),
                urls, feedback);
            presenter.Start();
            presenter.Open();

            view.ClickPrivacy();
            view.ClickTerms();
            view.ClickClose();
            view.ClickClose();

            Assert.That(urls.Opened, Is.EqualTo(new[] { "https://example.com/privacy", "https://example.com/terms" }));
            Assert.That(view.Hidden, Is.EqualTo(1));
            Assert.That(feedback.Clicks, Is.EqualTo(3));
            Assert.That(presenter.IsOpen, Is.False);
        }
    }
}
