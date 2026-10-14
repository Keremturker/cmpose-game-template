namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // A gem on the ground: its lane and its centre on the distance axis
    public sealed class Gem
    {
        public Gem(int lane, float z)
        {
            Lane = lane;
            Z = z;
        }

        public int Lane { get; }
        public float Z { get; }
    }
}
