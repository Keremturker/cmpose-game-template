using System;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    public interface IGameOverView
    {
        event Action RetryClicked;
        event Action MenuClicked;
        void Show(int score, int best, bool newBest);
        void Hide();
    }
}
