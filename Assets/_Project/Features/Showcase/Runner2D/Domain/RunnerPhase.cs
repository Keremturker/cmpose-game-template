namespace Kturker.UnityBase.Features.Showcase.Runner2D.Domain
{
    // Ready: waiting for the first touch. Crashed: waiting for Continue or Finish. Over: finished.
    public enum RunnerPhase
    {
        Ready,
        Running,
        Crashed,
        Over
    }
}
