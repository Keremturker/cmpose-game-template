using Kturker.UnityBase.Features.Showcase.Runner2D;
using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;

namespace Kturker.UnityBase.Tests.EditMode
{
    // The 2D runner's fakes; the internal 'runner2D' flag removes this file with the game
    public sealed class FakeRunnerWorldView : IRunnerWorldView
    {
        public int Renders;
        public RunnerSimulation Last;
        public bool? Premium;

        public void Render(RunnerSimulation sim)
        {
            Renders++;
            Last = sim;
        }

        public void SetPremium(bool premium) => Premium = premium;
    }

    public sealed class FakeTapInput : ITapInput
    {
        private bool pressed;
        public bool Held { get; set; }
        public void Tap() => pressed = true;

        public bool ConsumePress()
        {
            var was = pressed;
            pressed = false;
            return was;
        }
    }
}
