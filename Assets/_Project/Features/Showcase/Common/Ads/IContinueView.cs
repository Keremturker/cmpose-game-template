using System;

namespace Kturker.UnityBase.Features.Showcase.Common.Ads
{
    public interface IContinueView
    {
        event Action ContinueClicked;

        // viaAd: the label says the continue costs an ad ("Continue (ad)"); premium players see "Continue"
        void Show(bool viaAd);
        void Hide();

        // Hides the button and shows a one-line hint instead
        void ShowHint(string key);
    }
}
