using System;

namespace Kturker.UnityBase.Features.Sample.Presentation
{
    public interface ISampleView
    {
        string Label { get; }
        event Action BackClicked;
        event Action CompleteClicked;
        void ShowGreeting(string label);
        void ShowDone();
        void ShowCompleted(int count);
    }
}
