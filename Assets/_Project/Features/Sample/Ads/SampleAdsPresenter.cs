using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Ads;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Sample.Presentation;

namespace Kturker.UnityBase.Features.Sample.Ads
{
    // The ads flag's example in the Game scene: an interstitial after Complete (the pacer decides) and a Bonus button
    // whose rewarded ad adds one to the completion count. Leaving the scene cancels; a late result changes nothing.
    public sealed class SampleAdsPresenter : Presenter<IBonusView>
    {
        private readonly ISampleView sample;
        private readonly IAds ads;
        private readonly ICompletionCounter counter;
        private readonly IUiFeedback feedback;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private bool watching;

        public SampleAdsPresenter(IBonusView view, ISampleView sample, IAds ads, ICompletionCounter counter, IUiFeedback feedback)
            : base(view)
        {
            this.sample = sample;
            this.ads = ads;
            this.counter = counter;
            this.feedback = feedback;
        }

        public override void Start()
        {
            sample.CompleteClicked += OnComplete;
            View.BonusClicked += OnBonus;
            ads.Changed += ShowReadyHint;
            ShowReadyHint();
        }

        public override void Dispose()
        {
            sample.CompleteClicked -= OnComplete;
            View.BonusClicked -= OnBonus;
            ads.Changed -= ShowReadyHint;
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private void ShowReadyHint() => View.ShowHint(ads.IsAvailable ? AdsKeys.BonusHint : AdsKeys.Unavailable);

        // A natural break: the player finished something
        private void OnComplete() => ads.ShowInterstitialAsync(cancellation.Token).Forget();

        private void OnBonus()
        {
            if (watching) return;
            feedback.Click();
            BonusAsync().Forget();
        }

        private async UniTaskVoid BonusAsync()
        {
            watching = true;
            var token = cancellation.Token;
            RewardResult result;
            try
            {
                result = await ads.ShowRewardedAsync(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            if (token.IsCancellationRequested) return;
            watching = false;
            switch (result)
            {
                case RewardResult.Rewarded:
                case RewardResult.Granted:
                    var count = counter.Record();
                    if (count.HasValue) sample.ShowCompleted(count.Value);
                    ShowReadyHint();
                    break;
                case RewardResult.Dismissed:
                    ShowReadyHint();
                    break;
                default:
                    View.ShowHint(ads.IsAvailable ? AdsKeys.NotAvailable : AdsKeys.Unavailable);
                    break;
            }
        }
    }
}
