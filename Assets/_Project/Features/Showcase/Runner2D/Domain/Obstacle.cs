namespace Kturker.UnityBase.Features.Showcase.Runner2D.Domain
{
    // A box on the distance axis: X is its left edge, Bottom its lowest point above the ground
    public sealed class Obstacle
    {
        public Obstacle(ObstacleKind kind, float x, float width, float bottom, float height)
        {
            Kind = kind;
            X = x;
            Width = width;
            Bottom = bottom;
            Height = height;
        }

        public ObstacleKind Kind { get; }
        public float X { get; }
        public float Width { get; }
        public float Bottom { get; }
        public float Height { get; }
    }
}
