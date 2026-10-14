namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // Ready: waiting for the first touch or swipe. Crashed: waiting for Continue or Finish. Over: finished.
    public enum TrackPhase
    {
        Ready,
        Running,
        Crashed,
        Over
    }
}
