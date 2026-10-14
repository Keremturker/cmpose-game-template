using Kturker.UnityBase.Features.Showcase.Track3D.Domain;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    public interface ITrackWorldView
    {
        void Render(TrackSimulation sim);
        void SetPremium(bool premium);
    }
}
