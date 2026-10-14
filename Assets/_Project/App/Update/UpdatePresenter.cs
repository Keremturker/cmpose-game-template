using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Core.Update;

namespace Kturker.UnityBase.App.Update
{
    // Shows the update flow's decision on the menu. "Update required" cannot be closed (no Later, and the template
    // handles no back button); "Update available" closes with Later or Update and is not shown again until the next
    // check. A decision that arrives while the menu is open shows at once; a reloaded menu shows a standing one.
    public sealed class UpdatePresenter : Presenter<IUpdateView>
    {
        private readonly UpdateFlow flow;
        private readonly IPlatformUpdater platform;
        private readonly IUiFeedback feedback;
        private UpdateDecision shown;

        public UpdatePresenter(IUpdateView view, UpdateFlow flow, IPlatformUpdater platform, IUiFeedback feedback) : base(view)
        {
            this.flow = flow;
            this.platform = platform;
            this.feedback = feedback;
        }

        public override void Start()
        {
            View.UpdateClicked += OnUpdate;
            View.LaterClicked += OnLater;
            flow.Decided += Show;
            Show(flow.Current);
        }

        public override void Dispose()
        {
            View.UpdateClicked -= OnUpdate;
            View.LaterClicked -= OnLater;
            flow.Decided -= Show;
        }

        private void Show(UpdateDecision decision)
        {
            if (decision == null || decision.Kind == UpdateKind.None) return;
            shown = decision;
            if (decision.Kind == UpdateKind.Required) View.ShowRequired(platform.CanStartUpdate(decision.StoreUrl));
            else View.ShowAvailable();
        }

        private void OnUpdate()
        {
            if (shown == null) return;
            feedback.Click();
            platform.StartUpdate(shown.StoreUrl);
            if (shown.Kind == UpdateKind.Available) Close();
        }

        private void OnLater()
        {
            if (shown == null || shown.Kind != UpdateKind.Available) return;
            feedback.Click();
            Close();
        }

        private void Close()
        {
            shown = null;
            flow.Dismiss();
            View.Hide();
        }
    }
}
