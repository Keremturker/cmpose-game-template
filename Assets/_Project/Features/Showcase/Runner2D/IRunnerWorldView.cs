using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    public interface IRunnerWorldView
    {
        void Render(RunnerSimulation sim);
        void SetPremium(bool premium);
    }
}
