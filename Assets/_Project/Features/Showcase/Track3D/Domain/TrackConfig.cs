namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // Tuning in world units and seconds (the view draws one unit as one unit). Lanes 0..2 lie side by side with lane 1
    // in the middle; the distance axis runs forward; the ball's bottom is at height 0 on the ground.
    public sealed class TrackConfig
    {
        public float LaneWidth { get; set; } = 1.6f;
        // The ball slides one lane over in this time
        public float LaneChangeSeconds { get; set; } = 0.12f;
        public float BallSize { get; set; } = 0.8f;
        public float StartSpeed { get; set; } = 8f;
        public float Acceleration { get; set; } = 0.25f;
        public float MaxSpeed { get; set; } = 18f;
        // A jump is a fixed arc this long and this high; holding the touch changes nothing
        public float JumpSeconds { get; set; } = 0.6f;
        public float JumpHeight { get; set; } = 1.2f;
        public float FirstRowAt { get; set; } = 24f;
        // Start-to-start distance of rows at the start speed; it grows with the speed so the time between rows stays fair
        public float MinRowGap { get; set; } = 10f;
        public float MaxRowGap { get; set; } = 16f;
        // At least this long of open track between two rows, at the speed when the row is laid
        public float ReactionSeconds { get; set; } = 0.6f;
        public float GapChance { get; set; } = 0.25f;
        // A gap is this share of the distance one jump covers at the current speed
        public float GapJumpShare { get; set; } = 0.6f;
        public float TwoBlockChance { get; set; } = 0.5f;
        public float BlockLength { get; set; } = 1f;
        // Taller than a jump, for the view; blocks are never jumped over whatever the height
        public float BlockHeight { get; set; } = 1.6f;
        public float GemChance { get; set; } = 0.6f;
        public int MinGems { get; set; } = 3;
        public int MaxGems { get; set; } = 5;
        public float GemSpacing { get; set; } = 1.4f;
        public float GemSize { get; set; } = 0.5f;
        // The ball takes a gem in its lane while it is no higher than this
        public float GemReach { get; set; } = 0.6f;
        public int GemBonus { get; set; } = 10;
        // Shrinks the ball's length at both ends, so grazing a block's edge is not a crash
        public float Forgiveness { get; set; } = 0.15f;
        public float SpawnAhead { get; set; } = 70f;
        public float KeepBehind { get; set; } = 10f;
        // A fall into a gap shows this long before the crash
        public float FallSeconds { get; set; } = 0.4f;
        public float FallGravity { get; set; } = 25f;
        public float ContinueClearAhead { get; set; } = 24f;
        public float ContinueInvulnerable { get; set; } = 1f;
        // A long frame (a hitch, a breakpoint) moves the game at most this far. Keep MaxSpeed * MaxStep below the
        // smallest overlap window (gem: BallSize + GemSize; block: BlockLength + BallSize - 2 * Forgiveness; a gap's
        // length), or one step can skip past a gem or a block
        public float MaxStep { get; set; } = 0.05f;

        public float LaneX(int lane) => (lane - 1) * LaneWidth;
    }
}
