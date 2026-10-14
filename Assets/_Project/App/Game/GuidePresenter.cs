using Kturker.UnityBase.Core.Preferences;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.App.Game
{
    // Shown until "Got it" once; the dismissal is saved at once (guide.dismissed)
    public sealed class GuidePresenter : Presenter<IGuideView>
    {
        public const string DismissedKey = "guide.dismissed";

        private readonly IPreferences preferences;
        private readonly IUiFeedback feedback;
        private bool visible;

        public GuidePresenter(IGuideView view, IPreferences preferences, IUiFeedback feedback) : base(view)
        {
            this.preferences = preferences;
            this.feedback = feedback;
        }

        public override void Start()
        {
            View.DismissClicked += OnDismiss;
            if (preferences.GetBool(DismissedKey, false)) return;
            visible = true;
            View.Show();
        }

        public override void Dispose() => View.DismissClicked -= OnDismiss;

        private void OnDismiss()
        {
            if (!visible) return;
            visible = false;
            feedback.Click();
            View.Hide();
            preferences.SetBool(DismissedKey, true);
            preferences.Save();
        }
    }
}
