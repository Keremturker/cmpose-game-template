using Kturker.UnityBase.Features.Showcase.Track3D.Domain;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    // Swipes and taps as track inputs: Consume gives the oldest one not taken yet, or None
    public interface ITrackInput
    {
        TrackInput Consume();
    }
}
