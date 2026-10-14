using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Core.Review;

namespace Kturker.UnityBase.Features.Sample.Presentation
{
    public sealed class SamplePresenter : Presenter<ISampleView>
    {
        private readonly ISceneLoader loader;
        private readonly IUiFeedback feedback;
        private readonly ReviewPrompter review;
        private readonly ICompletionCounter counter;
        private readonly ILog log;

        public SamplePresenter(ISampleView view, ISceneLoader loader, IUiFeedback feedback, ReviewPrompter review, ICompletionCounter counter, ILog log) : base(view)
        {
            this.loader = loader;
            this.feedback = feedback;
            this.review = review;
            this.counter = counter;
            this.log = log;
        }

        public override void Start()
        {
            View.ShowGreeting(View.Label);
            var current = counter.Current;
            if (current.HasValue) View.ShowCompleted(current.Value);
            View.BackClicked += OnBack;
            View.CompleteClicked += OnComplete;
            log.Marker($"CMPOSE_FEATURE {View.Label}");
        }

        public override void Dispose()
        {
            View.BackClicked -= OnBack;
            View.CompleteClicked -= OnComplete;
        }

        private void OnBack()
        {
            feedback.Click();
            loader.LoadAsync(SceneId.Menu).Forget();
        }

        // A positive moment: the player finished something. Record it right after, never at launch or mid-play.
        private void OnComplete()
        {
            feedback.Click();
            View.ShowDone();
            var count = counter.Record();
            if (count.HasValue) View.ShowCompleted(count.Value);
            review.RecordPositiveEvent();
        }
    }
}
