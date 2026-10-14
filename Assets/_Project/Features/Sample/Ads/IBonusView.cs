using System;

namespace Kturker.UnityBase.Features.Sample.Ads
{
    public interface IBonusView
    {
        event Action BonusClicked;
        void ShowHint(string key);
    }
}
