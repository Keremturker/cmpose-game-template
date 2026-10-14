using System.Collections.Generic;
using System.Linq;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Track3D.Tests
{
    public sealed class TrackSimulationTests
    {
        private const float Frame = 0.01f;

        // Nothing on the track for a long way: for tests of lanes, the jump and the speed
        private static TrackConfig Open() => new TrackConfig { FirstRowAt = 1e6f };

        // Only gaps from distance 10, steady speed, no gems
        private static TrackConfig Gaps() => new TrackConfig { FirstRowAt = 10f, GapChance = 1f, GemChance = 0f, Acceleration = 0f };

        // Only rows of two blocks (one open lane) from distance 10, steady speed, no gems
        private static TrackConfig TwoBlocks() =>
            new TrackConfig { FirstRowAt = 10f, GapChance = 0f, TwoBlockChance = 1f, GemChance = 0f, Acceleration = 0f };

        // Only rows of one block (two open lanes) from distance 10 with a run of gems after each, steady speed
        private static TrackConfig OneBlockAndGems() =>
            new TrackConfig { FirstRowAt = 10f, GapChance = 0f, TwoBlockChance = 0f, GemChance = 1f, Acceleration = 0f };

        // A long stretch laid at once, for tests of the layout over many rows
        private static TrackConfig Long() => new TrackConfig { SpawnAhead = 1500f };

        private static TrackSimulation Started(TrackConfig config, int seed = 1)
        {
            var sim = new TrackSimulation(config, seed);
            Assert.That(sim.Step(Frame, TrackInput.Jump), Is.EqualTo(StepEvents.Started));
            return sim;
        }

        // Steps without input while the run lasts (a fall included); returns every event seen
        private static StepEvents Run(TrackSimulation sim, float seconds)
        {
            var seen = StepEvents.None;
            for (var t = 0f; t < seconds && sim.Phase == TrackPhase.Running; t += Frame) seen |= sim.Step(Frame, TrackInput.None);
            return seen;
        }

        // Steps without input until the ball reaches the distance, falls or the run stops
        private static void RunTo(TrackSimulation sim, float distance)
        {
            while (sim.Phase == TrackPhase.Running && !sim.Falling && sim.Distance < distance) sim.Step(Frame, TrackInput.None);
        }

        private static void SteerTo(TrackSimulation sim, int lane)
        {
            while (sim.Lane != lane) sim.Step(Frame, sim.Lane < lane ? TrackInput.Right : TrackInput.Left);
        }

        private static int Blocked(TrackRow row) => Enumerable.Range(0, TrackSimulation.Lanes).Count(row.Covers);

        [Test]
        public void Ready_waits_for_any_input_and_the_first_one_only_starts_the_run()
        {
            var sim = new TrackSimulation(Open(), 1);

            Assert.That(sim.Step(1f, TrackInput.None), Is.EqualTo(StepEvents.None));
            Assert.That((sim.Phase, sim.Distance), Is.EqualTo((TrackPhase.Ready, 0f)));

            Assert.That(sim.Step(Frame, TrackInput.Left), Is.EqualTo(StepEvents.Started));
            Assert.That((sim.Phase, sim.Lane, sim.Y, sim.Distance), Is.EqualTo((TrackPhase.Running, 1, 0f, 0f)));
        }

        [Test]
        public void The_track_ahead_is_laid_before_the_run_starts()
        {
            var config = new TrackConfig();
            var sim = new TrackSimulation(config, 3);

            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Ready));
            Assert.That(sim.Rows, Is.Not.Empty);
            Assert.That(sim.Rows[0].Start, Is.EqualTo(config.FirstRowAt));
            Assert.That(sim.Rows[sim.Rows.Count - 1].Start, Is.LessThan(config.SpawnAhead));
        }

        [Test]
        public void Swipes_change_lane_and_the_edge_lanes_ignore_a_further_move()
        {
            var sim = Started(Open());

            Assert.That(sim.Step(Frame, TrackInput.Left), Is.EqualTo(StepEvents.Moved));
            Assert.That(sim.Lane, Is.EqualTo(0));
            Assert.That(sim.Step(Frame, TrackInput.Left), Is.EqualTo(StepEvents.None));
            Assert.That(sim.Lane, Is.EqualTo(0));

            sim.Step(Frame, TrackInput.Right);
            sim.Step(Frame, TrackInput.Right);
            Assert.That(sim.Lane, Is.EqualTo(2));
            Assert.That(sim.Step(Frame, TrackInput.Right), Is.EqualTo(StepEvents.None));
            Assert.That(sim.Lane, Is.EqualTo(2));
        }

        [Test]
        public void The_ball_slides_into_the_new_lane_within_the_lane_change_time()
        {
            var config = Open();
            var sim = Started(config);

            sim.Step(Frame, TrackInput.Right);
            Assert.That(sim.X, Is.GreaterThan(0f).And.LessThan(config.LaneWidth));

            Run(sim, config.LaneChangeSeconds);
            Assert.That(sim.X, Is.EqualTo(config.LaneX(2)));
        }

        [Test]
        public void A_lane_change_in_the_air_is_allowed()
        {
            var sim = Started(Open());
            sim.Step(Frame, TrackInput.Jump);
            Run(sim, 0.1f);
            Assert.That(sim.Grounded, Is.False);

            Assert.That(sim.Step(Frame, TrackInput.Left), Is.EqualTo(StepEvents.Moved));
            Assert.That(sim.Lane, Is.EqualTo(0));
        }

        [Test]
        public void A_jump_is_a_fixed_arc_that_peaks_at_the_jump_height_and_lands_after_the_jump_time()
        {
            var config = Open();
            var sim = Started(config);

            Assert.That(sim.Step(Frame, TrackInput.Jump), Is.EqualTo(StepEvents.Jumped));
            var peak = sim.Y;
            var airborne = Frame;
            while (!sim.Grounded)
            {
                sim.Step(Frame, TrackInput.None);
                peak = System.Math.Max(peak, sim.Y);
                airborne += Frame;
            }

            Assert.That(peak, Is.EqualTo(config.JumpHeight).Within(0.01f));
            Assert.That(airborne, Is.EqualTo(config.JumpSeconds).Within(2 * Frame));
            Assert.That(sim.Y, Is.Zero);
        }

        [Test]
        public void A_jump_needs_the_ground_and_a_tap_in_the_air_does_not_stretch_the_arc()
        {
            var sim = Started(Open());
            sim.Step(Frame, TrackInput.Jump);
            Run(sim, 0.2f);

            Assert.That(sim.Step(Frame, TrackInput.Jump) & StepEvents.Jumped, Is.EqualTo(StepEvents.None));
            Run(sim, 0.45f);
            Assert.That(sim.Grounded, Is.True);
        }

        [Test]
        public void A_block_on_the_ball_lane_crashes_even_in_a_jump()
        {
            var config = TwoBlocks();
            var sim = Started(config);
            var row = sim.Rows[0];
            SteerTo(sim, Enumerable.Range(0, TrackSimulation.Lanes).First(row.Covers));
            RunTo(sim, row.Start - 1.5f);

            sim.Step(Frame, TrackInput.Jump);
            var seen = Run(sim, 1f);

            Assert.That(seen & StepEvents.Crashed, Is.EqualTo(StepEvents.Crashed));
            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Crashed));
            Assert.That(sim.Y, Is.GreaterThan(0f));
            Assert.That(sim.Distance, Is.LessThan(row.End + config.BallSize));
        }

        [Test]
        public void A_block_row_is_passed_in_its_open_lane()
        {
            var sim = Started(TwoBlocks());
            var row = sim.Rows[0];
            SteerTo(sim, Enumerable.Range(0, TrackSimulation.Lanes).First(lane => !row.Covers(lane)));

            RunTo(sim, row.End + 2f);

            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Running));
            Assert.That(sim.Distance, Is.GreaterThanOrEqualTo(row.End + 2f));
        }

        [Test]
        public void Rolling_onto_a_gap_falls_for_a_moment_and_then_crashes()
        {
            var config = Gaps();
            var sim = Started(config);
            var gap = sim.Rows[0];
            Assert.That(gap.Kind, Is.EqualTo(RowKind.Gap));

            RunTo(sim, gap.End);
            Assert.That((sim.Falling, sim.Phase), Is.EqualTo((true, TrackPhase.Running)));
            var at = sim.Distance;
            Assert.That(at, Is.GreaterThanOrEqualTo(gap.Start).And.LessThan(gap.Start + 1f));

            var seen = Run(sim, config.FallSeconds + 0.1f);
            Assert.That(seen & StepEvents.Crashed, Is.EqualTo(StepEvents.Crashed));
            Assert.That((sim.Phase, sim.Distance), Is.EqualTo((TrackPhase.Crashed, at)));
            Assert.That(sim.Y, Is.LessThan(-1f));
        }

        [Test]
        public void A_jump_just_before_a_gap_clears_it()
        {
            var sim = Started(Gaps());
            var gap = sim.Rows[0];
            RunTo(sim, gap.Start - 0.5f);

            sim.Step(Frame, TrackInput.Jump);
            RunTo(sim, gap.Start + 5f);

            Assert.That((sim.Phase, sim.Falling, sim.Grounded), Is.EqualTo((TrackPhase.Running, false, true)));
            Assert.That(sim.Distance, Is.GreaterThan(gap.End));
        }

        [Test]
        public void Gems_on_the_ball_lane_are_collected_and_add_the_bonus()
        {
            var config = OneBlockAndGems();
            var sim = Started(config);
            var run = sim.Gems.Where(g => g.Z < sim.Rows[1].Start).ToList();
            Assert.That(run.Count, Is.InRange(1, config.MaxGems));

            SteerTo(sim, run[0].Lane);
            RunTo(sim, run[run.Count - 1].Z + 1f);

            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Running));
            Assert.That(sim.Collected, Is.EqualTo(run.Count));
            Assert.That(sim.Score, Is.EqualTo((int)sim.Distance + run.Count * config.GemBonus));
        }

        [Test]
        public void Gems_on_another_lane_are_left()
        {
            var sim = Started(OneBlockAndGems());
            var row = sim.Rows[0];
            var run = sim.Gems.Where(g => g.Z < sim.Rows[1].Start).ToList();
            SteerTo(sim, Enumerable.Range(0, TrackSimulation.Lanes).First(lane => !row.Covers(lane) && lane != run[0].Lane));

            RunTo(sim, run[run.Count - 1].Z + 1f);

            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Running));
            Assert.That(sim.Collected, Is.Zero);
        }

        [Test]
        public void The_speed_grows_to_the_cap_and_stays_there()
        {
            var sim = Started(new TrackConfig { FirstRowAt = 1e6f, StartSpeed = 8f, Acceleration = 4f, MaxSpeed = 10f });

            Run(sim, 0.25f);
            Assert.That(sim.Speed, Is.GreaterThan(8f).And.LessThan(10f));
            Run(sim, 2f);
            Assert.That(sim.Speed, Is.EqualTo(10f));
        }

        [Test]
        public void Every_block_row_blocks_one_or_two_lanes_and_leaves_one_open()
        {
            var seen = new HashSet<int>();
            for (var seed = 0; seed < 20; seed++)
            {
                var sim = new TrackSimulation(Long(), seed);
                foreach (var row in sim.Rows.Where(r => r.Kind == RowKind.Blocks))
                {
                    Assert.That(Blocked(row), Is.InRange(1, 2), $"seed {seed}, row at {row.Start}");
                    seen.Add(Blocked(row));
                }
            }
            Assert.That(seen, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void Every_gap_is_shorter_than_a_jump_and_rows_leave_time_to_react()
        {
            var config = Long();
            var gaps = 0;
            for (var seed = 0; seed < 20; seed++)
            {
                var sim = new TrackSimulation(config, seed);
                for (var i = 0; i < sim.Rows.Count; i++)
                {
                    var row = sim.Rows[i];
                    if (row.Kind == RowKind.Gap)
                    {
                        gaps++;
                        Assert.That(Blocked(row), Is.EqualTo(TrackSimulation.Lanes), $"seed {seed}: a gap spans every lane");
                        Assert.That(row.Length, Is.LessThanOrEqualTo(config.GapJumpShare * config.StartSpeed * config.JumpSeconds + 1e-4f),
                            $"seed {seed}, gap at {row.Start}");
                    }
                    if (i == 0) continue;
                    Assert.That(row.Start - sim.Rows[i - 1].End, Is.GreaterThanOrEqualTo(config.ReactionSeconds * config.StartSpeed - 1e-3f),
                        $"seed {seed}, row at {row.Start}");
                }
            }
            Assert.That(gaps, Is.GreaterThan(0));
        }

        [Test]
        public void Gems_lie_on_open_ground_between_rows_in_runs_of_at_most_the_maximum()
        {
            var config = new TrackConfig { SpawnAhead = 1500f, GemChance = 1f };
            for (var seed = 0; seed < 20; seed++)
            {
                var sim = new TrackSimulation(config, seed);
                var runs = new Dictionary<int, int>();
                foreach (var gem in sim.Gems)
                {
                    var before = -1;
                    for (var i = 0; i < sim.Rows.Count && sim.Rows[i].End < gem.Z; i++) before = i;
                    Assert.That(before, Is.GreaterThanOrEqualTo(0), $"seed {seed}: gem at {gem.Z} before the first row");
                    var row = sim.Rows[before];
                    if (before + 1 < sim.Rows.Count) Assert.That(gem.Z, Is.LessThan(sim.Rows[before + 1].Start), $"seed {seed}: gem at {gem.Z}");
                    if (row.Kind == RowKind.Blocks) Assert.That(row.Covers(gem.Lane), Is.False, $"seed {seed}: gem at {gem.Z} behind a block");
                    runs[before] = runs.TryGetValue(before, out var count) ? count + 1 : 1;
                }
                Assert.That(runs, Is.Not.Empty);
                Assert.That(runs.Values, Has.All.InRange(1, config.MaxGems));
            }
        }

        [Test]
        public void Continue_after_a_block_crash_clears_the_rows_ahead_and_protects_the_ball_a_moment()
        {
            var config = TwoBlocks();
            var sim = Started(config);
            var row = sim.Rows[0];
            SteerTo(sim, Enumerable.Range(0, TrackSimulation.Lanes).First(row.Covers));
            Run(sim, 5f);
            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Crashed));

            Assert.That(sim.Continue(), Is.True);
            Assert.That((sim.Phase, sim.Continued, sim.Invulnerable), Is.EqualTo((TrackPhase.Running, true, true)));
            var from = sim.Distance - config.BallSize;
            var to = sim.Distance + config.ContinueClearAhead;
            Assert.That(sim.Rows.Any(r => r.Start < to && r.End > from), Is.False);

            Run(sim, config.ContinueInvulnerable * 0.9f);
            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Running));
        }

        [Test]
        public void Continue_after_a_fall_puts_the_ball_back_on_the_track_and_works_once_per_run()
        {
            var sim = Started(Gaps());
            Run(sim, 10f);
            Assert.That((sim.Phase, sim.Falling), Is.EqualTo((TrackPhase.Crashed, true)));

            Assert.That(sim.Continue(), Is.True);
            Assert.That((sim.Phase, sim.Falling, sim.Grounded, sim.Y), Is.EqualTo((TrackPhase.Running, false, true, 0f)));
            Assert.That(sim.Rows.Any(r => r.Start <= sim.Distance && r.End >= sim.Distance), Is.False);
            Run(sim, 0.5f);
            Assert.That((sim.Phase, sim.Falling), Is.EqualTo((TrackPhase.Running, false)));

            Run(sim, 30f);
            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Crashed));
            Assert.That(sim.Continue(), Is.False);
            Assert.That(sim.Phase, Is.EqualTo(TrackPhase.Crashed));
        }

        [Test]
        public void Continue_and_Finish_work_only_after_a_crash_and_an_over_run_ignores_input()
        {
            var running = Started(Open());
            Assert.That(running.Continue(), Is.False);
            Assert.That(running.Finish(), Is.False);
            Assert.That(running.Phase, Is.EqualTo(TrackPhase.Running));

            var crashed = Started(Gaps());
            Run(crashed, 10f);
            Assert.That(crashed.Finish(), Is.True);
            Assert.That(crashed.Finish(), Is.False);
            Assert.That(crashed.Continue(), Is.False);
            Assert.That(crashed.Phase, Is.EqualTo(TrackPhase.Over));
            Assert.That(crashed.Step(Frame, TrackInput.Jump), Is.EqualTo(StepEvents.None));
            Assert.That(crashed.Phase, Is.EqualTo(TrackPhase.Over));
        }

        [Test]
        public void A_long_frame_moves_at_most_one_max_step_and_a_zero_frame_ignores_input()
        {
            var config = Open();
            var sim = Started(config);

            sim.Step(1f, TrackInput.None);
            Assert.That(sim.Distance, Is.EqualTo(config.StartSpeed * config.MaxStep).Within(0.01f));

            var distance = sim.Distance;
            Assert.That(sim.Step(0f, TrackInput.Left), Is.EqualTo(StepEvents.None));
            Assert.That((sim.Lane, sim.Distance), Is.EqualTo((1, distance)));
        }

        [Test]
        public void At_top_speed_with_long_frames_a_block_still_crashes_and_a_gap_still_drops_the_ball()
        {
            var fast = new TrackConfig { StartSpeed = 18f, MaxSpeed = 18f, FirstRowAt = 20f, GapChance = 0f, TwoBlockChance = 1f, GemChance = 0f };
            var blocks = Started(fast);
            var row = blocks.Rows[0];
            SteerTo(blocks, Enumerable.Range(0, TrackSimulation.Lanes).First(row.Covers));
            for (var i = 0; i < 200 && blocks.Phase == TrackPhase.Running; i++) blocks.Step(1f, TrackInput.None);
            Assert.That(blocks.Phase, Is.EqualTo(TrackPhase.Crashed));
            Assert.That(blocks.Distance, Is.LessThan(row.End + fast.BallSize));

            fast.GapChance = 1f;
            var gaps = Started(fast);
            var gap = gaps.Rows[0];
            for (var i = 0; i < 200 && gaps.Phase == TrackPhase.Running; i++) gaps.Step(1f, TrackInput.None);
            Assert.That(gaps.Phase, Is.EqualTo(TrackPhase.Crashed));
            Assert.That(gaps.Distance, Is.InRange(gap.Start, gap.End));
        }

        [Test]
        public void The_same_seed_lays_the_same_track()
        {
            string Layout(int seed)
            {
                var sim = new TrackSimulation(new TrackConfig { SpawnAhead = 300f }, seed);
                return string.Join(";", sim.Rows.Select(r => $"{r.Kind}{r.Lanes}@{r.Start:0.000}"))
                    + "|" + string.Join(";", sim.Gems.Select(g => $"{g.Lane}@{g.Z:0.000}"));
            }

            Assert.That(Layout(7), Is.EqualTo(Layout(7)));
            Assert.That(Layout(7), Is.Not.EqualTo(Layout(8)));
        }
    }
}
