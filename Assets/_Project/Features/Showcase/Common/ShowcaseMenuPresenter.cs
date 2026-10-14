using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    public sealed class ShowcaseMenuPresenter : Presenter<IShowcaseMenuView>
    {
        private readonly BestScore best;

        public ShowcaseMenuPresenter(IShowcaseMenuView view, BestScore best) : base(view) => this.best = best;

        public override void Start() => View.ShowBest(best.Value);
    }
}
