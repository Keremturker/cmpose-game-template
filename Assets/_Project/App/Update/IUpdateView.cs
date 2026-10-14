using System;

namespace Kturker.UnityBase.App.Update
{
    public interface IUpdateView
    {
        event Action UpdateClicked;
        event Action LaterClicked;

        // canUpdate false: no Update button, and the message asks to update from the App Store
        void ShowRequired(bool canUpdate);

        void ShowAvailable();

        void Hide();
    }
}
