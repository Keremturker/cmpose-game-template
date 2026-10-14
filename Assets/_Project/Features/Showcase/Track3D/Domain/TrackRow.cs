namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // A row across the track from Start for Length on the distance axis; Lanes has bit i set when lane i is blocked
    // (every bit for a gap)
    public sealed class TrackRow
    {
        public TrackRow(RowKind kind, float start, float length, int lanes)
        {
            Kind = kind;
            Start = start;
            Length = length;
            Lanes = lanes;
        }

        public RowKind Kind { get; }
        public float Start { get; }
        public float Length { get; }
        public int Lanes { get; }
        public float End => Start + Length;

        public bool Covers(int lane) => (Lanes & (1 << lane)) != 0;
    }
}
