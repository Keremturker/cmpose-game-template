using System;

namespace Kturker.UnityBase.Features.Showcase.Common.Purchases
{
    public interface IPremiumButtonView
    {
        event Action Clicked;
        void SetVisible(bool visible);
    }
}
