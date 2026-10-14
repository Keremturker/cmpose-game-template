using System;
using System.Collections.Generic;

namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // The whole 3D track without Unity: lanes, the jump arc, speed, rows of blocks and gaps, gems and collisions in
    // world units and seconds. The ball rolls at Distance in Lane (X slides to the lane's centre); rows and gems keep
    // fixed positions on the same axis. Collisions use the lane the ball is heading for. A seed makes a track
    // repeatable; the view only reads the state after each Step.
    public sealed class TrackSimulation
    {
        public const int Lanes = 3;
        private const int AllLanes = (1 << Lanes) - 1;

        private readonly TrackConfig config;
        private readonly Random random;
        private readonly List<TrackRow> rows = new List<TrackRow>();
        private readonly List<Gem> gems = new List<Gem>();
        private float nextRowAt;
        private float invulnerable;
        private float jumpTime;
        private float fallTime;
        private bool jumping;

        public TrackSimulation(TrackConfig config, int seed)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            random = new Random(seed);
            Speed = config.StartSpeed;
            nextRowAt = config.FirstRowAt;
            Lane = 1;
            // The track ahead is there before the first touch, so the ready screen shows it
            Spawn();
        }

        public TrackConfig Config => config;
        public TrackPhase Phase { get; private set; } = TrackPhase.Ready;
        public float Distance { get; private set; }
        public int Lane { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Speed { get; private set; }
        public int Collected { get; private set; }
        public bool Continued { get; private set; }
        public bool Falling { get; private set; }
        public bool Invulnerable => invulnerable > 0f;
        public bool Grounded => !jumping && !Falling;
        public IReadOnlyList<TrackRow> Rows => rows;
        public IReadOnlyList<Gem> Gems => gems;
        public int Score => (int)Distance + Collected * config.GemBonus;

        // input: what the player did since the last step. A frame of zero seconds does not move a running track, but
        // input still starts a ready one; the presenter drops input on paused frames
        public StepEvents Step(float seconds, TrackInput input)
        {
            if (Phase == TrackPhase.Ready)
            {
                if (input == TrackInput.None) return StepEvents.None;
                Phase = TrackPhase.Running;
                return StepEvents.Started;
            }
            if (Phase != TrackPhase.Running || seconds <= 0f) return StepEvents.None;
            var dt = Math.Min(seconds, config.MaxStep);
            if (Falling) return Fall(dt);
            var events = Steer(input);
            Slide(dt);
            Arc(dt);
            Speed = Math.Min(config.MaxSpeed, Speed + config.Acceleration * dt);
            Distance += Speed * dt;
            invulnerable = Math.Max(0f, invulnerable - dt);
            Spawn();
            Despawn();
            if (CollectGems()) events |= StepEvents.GemCollected;
            if (Invulnerable) return events;
            if (HitsBlock())
            {
                Phase = TrackPhase.Crashed;
                return events | StepEvents.Crashed;
            }
            if (Grounded && OverGap())
            {
                Falling = true;
                fallTime = 0f;
            }
            return events;
        }

        // After a crash, once per run: clears the rows at and ahead of the ball, puts a fallen ball back on the track
        // and protects it a moment
        public bool Continue()
        {
            if (Phase != TrackPhase.Crashed || Continued) return false;
            Continued = true;
            var from = Distance - config.BallSize;
            var to = Distance + config.ContinueClearAhead;
            rows.RemoveAll(r => r.Start < to && r.End > from);
            if (Falling)
            {
                Falling = false;
                fallTime = 0f;
                Y = 0f;
            }
            invulnerable = config.ContinueInvulnerable;
            Phase = TrackPhase.Running;
            return true;
        }

        public bool Finish()
        {
            if (Phase != TrackPhase.Crashed) return false;
            Phase = TrackPhase.Over;
            return true;
        }

        private StepEvents Steer(TrackInput input)
        {
            switch (input)
            {
                case TrackInput.Left when Lane > 0:
                    Lane--;
                    return StepEvents.Moved;
                case TrackInput.Right when Lane < Lanes - 1:
                    Lane++;
                    return StepEvents.Moved;
                case TrackInput.Jump when Grounded:
                    jumping = true;
                    jumpTime = 0f;
                    return StepEvents.Jumped;
                default:
                    return StepEvents.None;
            }
        }

        // X moves to the lane's centre at a pace that crosses one lane in LaneChangeSeconds
        private void Slide(float dt)
        {
            var target = config.LaneX(Lane);
            var pace = config.LaneWidth / config.LaneChangeSeconds * dt;
            X = Math.Abs(target - X) <= pace ? target : X + Math.Sign(target - X) * pace;
        }

        private void Arc(float dt)
        {
            if (!jumping) return;
            jumpTime += dt;
            if (jumpTime >= config.JumpSeconds)
            {
                jumping = false;
                Y = 0f;
                return;
            }
            var p = jumpTime / config.JumpSeconds;
            Y = 4f * config.JumpHeight * p * (1f - p);
        }

        // The ball sinks where it fell (no forward motion, no input), then the run crashes
        private StepEvents Fall(float dt)
        {
            fallTime += dt;
            Y = -0.5f * config.FallGravity * fallTime * fallTime;
            if (fallTime < config.FallSeconds) return StepEvents.None;
            Phase = TrackPhase.Crashed;
            return StepEvents.Crashed;
        }

        private void Spawn()
        {
            while (nextRowAt < Distance + config.SpawnAhead)
            {
                var row = random.NextDouble() < config.GapChance
                    ? new TrackRow(RowKind.Gap, nextRowAt, config.GapJumpShare * Speed * config.JumpSeconds, AllLanes)
                    : new TrackRow(RowKind.Blocks, nextRowAt, config.BlockLength, BlockLanes());
                rows.Add(row);
                var rowGap = (config.MinRowGap + (config.MaxRowGap - config.MinRowGap) * (float)random.NextDouble())
                    * Math.Max(1f, Speed / config.StartSpeed);
                var next = Math.Max(row.Start + rowGap, row.End + config.ReactionSeconds * Speed);
                if (random.NextDouble() < config.GemChance) LayGems(row, next);
                nextRowAt = next;
            }
        }

        // One or two blocked lanes, never all three
        private int BlockLanes()
        {
            var first = random.Next(Lanes);
            var lanes = 1 << first;
            if (random.NextDouble() < config.TwoBlockChance) lanes |= 1 << ((first + 1 + random.Next(Lanes - 1)) % Lanes);
            return lanes;
        }

        // A run of gems on the ground just after the row, in a lane the row leaves open, ending before the next row
        private void LayGems(TrackRow row, float nextRow)
        {
            var lane = OpenLane(row);
            var count = random.Next(config.MinGems, config.MaxGems + 1);
            for (var i = 0; i < count; i++)
            {
                var z = row.End + config.GemSpacing * (i + 1);
                if (z > nextRow - config.GemSpacing) return;
                gems.Add(new Gem(lane, z));
            }
        }

        // A random lane the row does not block (any lane after a gap)
        private int OpenLane(TrackRow row)
        {
            var open = 0;
            for (var lane = 0; lane < Lanes; lane++)
            {
                if (IsOpen(row, lane)) open++;
            }
            var pick = random.Next(open);
            for (var lane = 0; lane < Lanes; lane++)
            {
                if (!IsOpen(row, lane)) continue;
                if (pick == 0) return lane;
                pick--;
            }
            return 1;
        }

        private static bool IsOpen(TrackRow row, int lane) => row.Kind == RowKind.Gap || !row.Covers(lane);

        // Rows and gems are sorted along the track, so the passed ones are at the front
        private void Despawn()
        {
            var behind = Distance - config.KeepBehind;
            while (rows.Count > 0 && rows[0].End < behind) rows.RemoveAt(0);
            while (gems.Count > 0 && gems[0].Z < behind) gems.RemoveAt(0);
        }

        private bool CollectGems()
        {
            if (Y > config.GemReach) return false;
            var reach = (config.BallSize + config.GemSize) / 2f;
            var collected = false;
            for (var i = gems.Count - 1; i >= 0; i--)
            {
                var gem = gems[i];
                if (gem.Lane != Lane || Math.Abs(gem.Z - Distance) > reach) continue;
                gems.RemoveAt(i);
                Collected++;
                collected = true;
            }
            return collected;
        }

        // Height does not matter: blocks are taller than any jump
        private bool HitsBlock()
        {
            var back = Distance - config.BallSize / 2f + config.Forgiveness;
            var front = Distance + config.BallSize / 2f - config.Forgiveness;
            foreach (var row in rows)
            {
                if (row.Kind == RowKind.Blocks && row.Covers(Lane) && back < row.End && front > row.Start) return true;
            }
            return false;
        }

        // The ball's centre over a gap
        private bool OverGap()
        {
            foreach (var row in rows)
            {
                if (row.Kind == RowKind.Gap && Distance >= row.Start && Distance <= row.End) return true;
            }
            return false;
        }
    }
}
