using System;

namespace Kturker.UnityBase.App.About
{
    public interface IAboutView
    {
        event Action PrivacyClicked;
        event Action TermsClicked;
        event Action CloseClicked;
        void Show(string version, bool privacyVisible, bool termsVisible, string licenses);
        void Hide();
    }
}
