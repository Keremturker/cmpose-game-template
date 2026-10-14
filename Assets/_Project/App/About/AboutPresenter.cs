using Kturker.UnityBase.Core.About;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.App.About
{
    public sealed class AboutPresenter : Presenter<IAboutView>, IAboutPanel
    {
        private readonly AboutInfo info;
        private readonly IUrlOpener urls;
        private readonly IUiFeedback feedback;

        public AboutPresenter(IAboutView view, AboutInfo info, IUrlOpener urls, IUiFeedback feedback) : base(view)
        {
            this.info = info;
            this.urls = urls;
            this.feedback = feedback;
        }

        public bool IsOpen { get; private set; }

        public override void Start()
        {
            View.PrivacyClicked += OnPrivacy;
            View.TermsClicked += OnTerms;
            View.CloseClicked += OnClose;
        }

        public override void Dispose()
        {
            View.PrivacyClicked -= OnPrivacy;
            View.TermsClicked -= OnTerms;
            View.CloseClicked -= OnClose;
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            View.Show(info.VersionLabel, info.HasPrivacy, info.HasTerms, info.Licenses);
        }

        private void OnPrivacy()
        {
            feedback.Click();
            if (info.HasPrivacy) urls.Open(info.PrivacyUrl);
        }

        private void OnTerms()
        {
            feedback.Click();
            if (info.HasTerms) urls.Open(info.TermsUrl);
        }

        private void OnClose()
        {
            if (!IsOpen) return;
            IsOpen = false;
            feedback.Click();
            View.Hide();
        }
    }
}
