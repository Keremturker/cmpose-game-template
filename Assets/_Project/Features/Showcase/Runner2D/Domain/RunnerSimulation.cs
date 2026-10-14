using System;
using System.Collections.Generic;

namespace Kturker.UnityBase.Features.Showcase.Runner2D.Domain
{
    // The whole runner without Unity: jump, speed, spawns, stars and collisions in world units and seconds. The runner
    // stands at x = Distance; obstacles and stars keep fixed x positions on the same axis. A seed makes a course
    // repeatable; the view only reads the state after each Step.
    public sealed class RunnerSimulation
    {
        private readonly RunnerConfig config;
        private readonly Random random;
        private readonly List<Obstacle> obstacles = new List<Obstacle>();
        private readonly List<Star> stars = new List<Star>();
        private float nextSpawnAt;
        private float invulnerable;

        public RunnerSimulation(RunnerConfig config, int seed)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            random = new Random(seed);
            Speed = config.StartSpeed;
            nextSpawnAt = config.FirstObstacleAt;
        }

        public RunnerConfig Config => config;
        public RunnerPhase Phase { get; private set; } = RunnerPhase.Ready;
        public float Distance { get; private set; }
        public float RunnerY { get; private set; }
        public float VelocityY { get; private set; }
        public float Speed { get; private set; }
        public int Collected { get; private set; }
        public bool Continued { get; private set; }
        public bool Invulnerable => invulnerable > 0f;
        public bool Grounded => RunnerY <= 0f && VelocityY <= 0f;
        public IReadOnlyList<Obstacle> Obstacles => obstacles;
        public IReadOnlyList<Star> Stars => stars;
        public int Score => (int)Distance + Collected * config.StarBonus;

        private float Left => Distance - config.RunnerSize / 2f + config.Forgiveness;
        private float Right => Distance + config.RunnerSize / 2f - config.Forgiveness;
        private float Bottom => RunnerY + config.Forgiveness;
        private float Top => RunnerY + config.RunnerSize - config.Forgiveness;

        // pressed: the screen was touched since the last step; held: it is still touched
        public StepEvents Step(float seconds, bool pressed, bool held)
        {
            if (Phase == RunnerPhase.Ready)
            {
                if (!pressed) return StepEvents.None;
                Phase = RunnerPhase.Running;
                return StepEvents.Started;
            }
            if (Phase != RunnerPhase.Running || seconds <= 0f) return StepEvents.None;
            var dt = Math.Min(seconds, config.MaxStep);
            var events = StepEvents.None;
            if (pressed && Grounded)
            {
                VelocityY = config.JumpSpeed;
                events |= StepEvents.Jumped;
            }
            var gravity = config.Gravity * (held && VelocityY > 0f ? config.HoldGravityScale : 1f);
            VelocityY -= gravity * dt;
            RunnerY += VelocityY * dt;
            if (RunnerY <= 0f)
            {
                RunnerY = 0f;
                VelocityY = 0f;
            }
            Speed = Math.Min(config.MaxSpeed, Speed + config.Acceleration * dt);
            Distance += Speed * dt;
            invulnerable = Math.Max(0f, invulnerable - dt);
            Spawn();
            Despawn();
            if (CollectStars()) events |= StepEvents.StarCollected;
            if (!Invulnerable && HitsObstacle())
            {
                Phase = RunnerPhase.Crashed;
                events |= StepEvents.Crashed;
            }
            return events;
        }

        // After a crash, once per run: clears the obstacles at and just ahead of the runner and protects it a moment
        public bool Continue()
        {
            if (Phase != RunnerPhase.Crashed || Continued) return false;
            Continued = true;
            var from = Distance - config.RunnerSize;
            var to = Distance + config.ContinueClearAhead;
            obstacles.RemoveAll(o => o.X < to && o.X + o.Width > from);
            invulnerable = config.ContinueInvulnerable;
            Phase = RunnerPhase.Running;
            return true;
        }

        public bool Finish()
        {
            if (Phase != RunnerPhase.Crashed) return false;
            Phase = RunnerPhase.Over;
            return true;
        }

        private void Spawn()
        {
            while (nextSpawnAt < Distance + config.SpawnAhead)
            {
                obstacles.Add(random.NextDouble() < config.HighChance
                    ? new Obstacle(ObstacleKind.High, nextSpawnAt, config.HighWidth, config.HighBottom, config.HighHeight)
                    : new Obstacle(ObstacleKind.Low, nextSpawnAt, config.LowWidth, 0f, config.LowHeight));
                var gap = (config.MinGap + (config.MaxGap - config.MinGap) * (float)random.NextDouble())
                    * Math.Max(1f, Speed / config.StartSpeed);
                if (random.NextDouble() < config.StarChance)
                {
                    var y = random.NextDouble() < 0.5 ? config.StarLowY : config.StarHighY;
                    stars.Add(new Star(nextSpawnAt + gap / 2f, y));
                }
                nextSpawnAt += gap;
            }
        }

        private void Despawn()
        {
            var behind = Distance - config.KeepBehind;
            obstacles.RemoveAll(o => o.X + o.Width < behind);
            stars.RemoveAll(s => s.X < behind);
        }

        private bool CollectStars()
        {
            var half = config.StarSize / 2f;
            var collected = stars.RemoveAll(s => Overlaps(s.X - half, s.X + half, s.Y - half, s.Y + half));
            Collected += collected;
            return collected > 0;
        }

        private bool HitsObstacle()
        {
            foreach (var o in obstacles)
            {
                if (Overlaps(o.X, o.X + o.Width, o.Bottom, o.Bottom + o.Height)) return true;
            }
            return false;
        }

        private bool Overlaps(float left, float right, float bottom, float top) =>
            Left < right && Right > left && Bottom < top && Top > bottom;
    }
}
