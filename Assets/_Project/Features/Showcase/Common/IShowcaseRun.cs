using System;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // What a showcase game offers the shared parts. A crash stops the run until it continues (once per run, the ads
    // flag offers it) or finishes; Finished fires exactly once per run, after Retry or Menu. Restart starts a new run
    // after a finished one; any call out of turn does nothing.
    public interface IShowcaseRun
    {
        bool IsCrashed { get; }
        bool CanContinue { get; }
        event Action<RunResult> Crashed;
        event Action Continued;
        event Action<RunResult> Finished;
        void Continue();
        void Finish();
        void Restart();
    }
}
