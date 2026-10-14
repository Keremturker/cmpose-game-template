using System;

namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // What happened during one Step (sounds, the haptic and the run's events follow these)
    [Flags]
    public enum StepEvents
    {
        None = 0,
        Started = 1,
        Jumped = 2,
        Moved = 4,
        GemCollected = 8,
        Crashed = 16
    }
}
