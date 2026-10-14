using System;

namespace Kturker.UnityBase.App.Game
{
    public interface IGuideView
    {
        event Action DismissClicked;
        void Show();
        void Hide();
    }
}
