namespace Kturker.UnityBase.Features.Showcase.Runner2D.Domain
{
    // Tuning in world units and seconds (the view draws one unit as one unit). The runner's feet are at y = 0.
    public sealed class RunnerConfig
    {
        public float Gravity { get; set; } = 40f;
        // While the touch is held and the runner still rises, gravity is scaled by this: a held jump goes higher
        public float HoldGravityScale { get; set; } = 0.5f;
        public float JumpSpeed { get; set; } = 13f;
        public float RunnerSize { get; set; } = 0.8f;
        public float StartSpeed { get; set; } = 6f;
        public float Acceleration { get; set; } = 0.15f;
        public float MaxSpeed { get; set; } = 14f;
        public float FirstObstacleAt { get; set; } = 12f;
        // Gaps at the start speed; they grow with the speed so the time between obstacles stays fair
        public float MinGap { get; set; } = 7f;
        public float MaxGap { get; set; } = 12f;
        public float SpawnAhead { get; set; } = 14f;
        public float KeepBehind { get; set; } = 4f;
        public float LowWidth { get; set; } = 0.7f;
        public float LowHeight { get; set; } = 0.9f;
        // A hanging obstacle: the runner passes under it on the ground and hits it when it jumps
        public float HighWidth { get; set; } = 0.9f;
        public float HighBottom { get; set; } = 1.1f;
        public float HighHeight { get; set; } = 2f;
        public float HighChance { get; set; } = 0.35f;
        public float StarChance { get; set; } = 0.6f;
        public float StarSize { get; set; } = 0.5f;
        public float StarLowY { get; set; } = 0.3f;
        public float StarHighY { get; set; } = 2.2f;
        public int StarBonus { get; set; } = 10;
        // Shrinks the runner's box on every side, so grazing an edge is not a crash
        public float Forgiveness { get; set; } = 0.1f;
        public float ContinueClearAhead { get; set; } = 6f;
        public float ContinueInvulnerable { get; set; } = 1f;
        // A long frame (a hitch, a breakpoint) moves the game at most this far
        public float MaxStep { get; set; } = 0.05f;
    }
}
