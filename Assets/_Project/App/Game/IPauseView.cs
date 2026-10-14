using System;

namespace Kturker.UnityBase.App.Game
{
    public interface IPauseView
    {
        event Action PauseClicked;
        event Action ResumeClicked;
        event Action SettingsClicked;
        event Action MenuClicked;
        void ShowPanel();
        void HidePanel();
    }
}
