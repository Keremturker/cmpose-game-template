namespace Kturker.UnityBase.Features.Showcase.Runner2D.Domain
{
    // A star's centre on the distance axis and above the ground
    public sealed class Star
    {
        public Star(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }
}
