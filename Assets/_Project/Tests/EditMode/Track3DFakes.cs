using System.Collections.Generic;
using Kturker.UnityBase.Features.Showcase.Track3D;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;

namespace Kturker.UnityBase.Tests.EditMode
{
    // The 3D track's fakes; the internal 'track3D' flag removes this file with the game
    public sealed class FakeTrackWorldView : ITrackWorldView
    {
        public int Renders;
        public TrackSimulation Last;
        public bool? Premium;

        public void Render(TrackSimulation sim)
        {
            Renders++;
            Last = sim;
        }

        public void SetPremium(bool premium) => Premium = premium;
    }

    public sealed class FakeTrackInput : ITrackInput
    {
        private readonly Queue<TrackInput> queued = new Queue<TrackInput>();
        public void Push(TrackInput input) => queued.Enqueue(input);
        public TrackInput Consume() => queued.Count > 0 ? queued.Dequeue() : TrackInput.None;
    }
}
