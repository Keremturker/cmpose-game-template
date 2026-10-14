using System.Linq;
using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Runner2D.Tests
{
    public sealed class RunnerSimulationTests
    {
        private const float Frame = 0.01f;

        // No obstacle for a long way: for tests of the jump and the speed
        private static RunnerConfig Open() => new RunnerConfig { FirstObstacleAt = 1e6f };

        private static RunnerSimulation Started(RunnerConfig config, int seed = 1)
        {
            var sim = new RunnerSimulation(config, seed);
            Assert.That(sim.Step(Frame, true, false), Is.EqualTo(StepEvents.Started));
            return sim;
        }

        // Steps without input while the run lasts; returns every event seen
        private static StepEvents Run(RunnerSimulation sim, float seconds)
        {
            var seen = StepEvents.None;
            for (var t = 0f; t < seconds && sim.Phase == RunnerPhase.Running; t += Frame) seen |= sim.Step(Frame, false, false);
            return seen;
        }

        private static void RunTo(RunnerSimulation sim, float distance)
        {
            while (sim.Phase == RunnerPhase.Running && sim.Distance < distance) sim.Step(Frame, false, false);
        }

        [Test]
        public void Ready_waits_for_the_first_touch_which_starts_without_jumping()
        {
            var sim = new RunnerSimulation(Open(), 1);

            Assert.That(sim.Step(1f, false, false), Is.EqualTo(StepEvents.None));
            Assert.That((sim.Phase, sim.Distance), Is.EqualTo((RunnerPhase.Ready, 0f)));

            Assert.That(sim.Step(Frame, true, true), Is.EqualTo(StepEvents.Started));
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Running));
            Assert.That(sim.VelocityY, Is.Zero);
        }

        [Test]
        public void A_touch_on_the_ground_jumps_and_the_runner_lands_again()
        {
            var sim = Started(Open());

            Assert.That(sim.Step(Frame, true, false) & StepEvents.Jumped, Is.EqualTo(StepEvents.Jumped));
            Assert.That(sim.RunnerY, Is.GreaterThan(0f));
            Run(sim, 1f);

            Assert.That(sim.Grounded, Is.True);
            Assert.That(sim.RunnerY, Is.Zero);
        }

        [Test]
        public void Holding_the_touch_jumps_higher()
        {
            float Peak(bool hold)
            {
                var sim = Started(Open());
                sim.Step(Frame, true, hold);
                var peak = 0f;
                for (var i = 0; i < 200; i++)
                {
                    sim.Step(Frame, false, hold);
                    peak = System.Math.Max(peak, sim.RunnerY);
                }
                return peak;
            }

            Assert.That(Peak(true), Is.GreaterThan(Peak(false) * 1.5f));
        }

        [Test]
        public void A_touch_in_the_air_does_not_jump_again()
        {
            var sim = Started(Open());
            sim.Step(Frame, true, false);
            Run(sim, 0.1f);
            var velocity = sim.VelocityY;

            Assert.That(sim.Step(Frame, true, false) & StepEvents.Jumped, Is.EqualTo(StepEvents.None));
            Assert.That(sim.VelocityY, Is.LessThan(velocity));
        }

        [Test]
        public void Speed_grows_to_its_cap_and_distance_follows()
        {
            var config = Open();
            var sim = Started(config);

            Run(sim, 1f);
            Assert.That(sim.Speed, Is.GreaterThan(config.StartSpeed).And.LessThan(config.MaxSpeed));
            Run(sim, 120f);

            Assert.That(sim.Speed, Is.EqualTo(config.MaxSpeed));
            Assert.That(sim.Distance, Is.GreaterThan(config.MaxSpeed * 60f));
        }

        [Test]
        public void A_long_frame_is_clamped_to_one_max_step()
        {
            var config = Open();
            var sim = Started(config);

            sim.Step(1f, false, false);

            Assert.That(sim.Distance, Is.EqualTo(config.StartSpeed * config.MaxStep).Within(0.001f));
        }

        [Test]
        public void The_same_seed_builds_the_same_course()
        {
            string Course(int seed)
            {
                var sim = Started(new RunnerConfig { HighChance = 1f }, seed);
                Run(sim, 2f);
                return string.Join(";", sim.Obstacles.Select(o => $"{o.Kind}@{o.X:F3}")) + "|" +
                    string.Join(";", sim.Stars.Select(s => $"{s.X:F3},{s.Y:F2}"));
            }

            Assert.That(Course(7), Is.EqualTo(Course(7)));
            Assert.That(Course(7), Is.Not.EqualTo(Course(8)));
        }

        [Test]
        public void Gaps_never_drop_below_the_minimum()
        {
            var config = new RunnerConfig { HighChance = 1f };
            var sim = Started(config, 3);
            var seen = new System.Collections.Generic.SortedSet<float>();

            for (var t = 0f; t < 20f && sim.Phase == RunnerPhase.Running; t += Frame)
            {
                sim.Step(Frame, false, false);
                foreach (var o in sim.Obstacles) seen.Add(o.X);
            }

            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Running));
            var xs = seen.ToList();
            Assert.That(xs.Count, Is.GreaterThan(5));
            for (var i = 1; i < xs.Count; i++) Assert.That(xs[i] - xs[i - 1], Is.GreaterThanOrEqualTo(config.MinGap - 0.0001f));
        }

        [Test]
        public void Running_into_a_low_obstacle_crashes_and_stops_the_run()
        {
            var sim = Started(new RunnerConfig { FirstObstacleAt = 3f, HighChance = 0f });

            var seen = Run(sim, 5f);

            Assert.That(seen & StepEvents.Crashed, Is.EqualTo(StepEvents.Crashed));
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Crashed));
            var distance = sim.Distance;
            Assert.That(sim.Step(Frame, true, true), Is.EqualTo(StepEvents.None));
            Assert.That(sim.Distance, Is.EqualTo(distance));
        }

        [Test]
        public void Jumping_over_a_low_obstacle_survives()
        {
            var sim = Started(new RunnerConfig { FirstObstacleAt = 5f, HighChance = 0f });

            RunTo(sim, 4f);
            sim.Step(Frame, true, false);
            Run(sim, 0.8f);

            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Running));
            Assert.That(sim.Distance, Is.GreaterThan(6.5f));
        }

        [Test]
        public void Running_under_a_high_obstacle_survives()
        {
            var sim = Started(new RunnerConfig { FirstObstacleAt = 5f, HighChance = 1f });

            Run(sim, 2f);

            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Running));
            Assert.That(sim.Distance, Is.GreaterThan(7f));
        }

        [Test]
        public void Jumping_into_a_high_obstacle_crashes()
        {
            var sim = Started(new RunnerConfig { FirstObstacleAt = 5f, HighChance = 1f });

            RunTo(sim, 4f);
            sim.Step(Frame, true, false);
            Run(sim, 0.8f);

            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Crashed));
        }

        [Test]
        public void Stars_on_the_path_are_collected_once_and_add_to_the_score()
        {
            var config = new RunnerConfig { FirstObstacleAt = 5f, HighChance = 1f, StarChance = 1f, StarHighY = 0.3f };
            var sim = Started(config);
            sim.Step(Frame, false, false);
            var firstStar = sim.Stars[0].X;

            var events = 0;
            while (sim.Distance < firstStar + 1f && sim.Phase == RunnerPhase.Running)
            {
                if ((sim.Step(Frame, false, false) & StepEvents.StarCollected) != 0) events++;
            }

            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Running));
            Assert.That(sim.Collected, Is.GreaterThanOrEqualTo(1));
            Assert.That(events, Is.EqualTo(sim.Collected));
            Assert.That(sim.Stars.All(s => s.X > sim.Distance - config.StarSize), Is.True);
            Assert.That(sim.Score, Is.EqualTo((int)sim.Distance + sim.Collected * config.StarBonus));
        }

        [Test]
        public void Continue_clears_ahead_protects_and_works_once()
        {
            var config = new RunnerConfig { FirstObstacleAt = 3f, HighChance = 0f };
            var sim = Started(config);
            Run(sim, 5f);
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Crashed));

            Assert.That(sim.Continue(), Is.True);
            Assert.That((sim.Phase, sim.Continued, sim.Invulnerable), Is.EqualTo((RunnerPhase.Running, true, true)));
            Assert.That(sim.Obstacles.Any(o => o.X < sim.Distance + config.ContinueClearAhead && o.X + o.Width > sim.Distance - config.RunnerSize),
                Is.False);
            Run(sim, 0.5f);
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Running));

            Run(sim, 30f);
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Crashed));
            Assert.That(sim.Continue(), Is.False);
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Crashed));
        }

        [Test]
        public void Finish_ends_only_a_crashed_run()
        {
            var sim = new RunnerSimulation(new RunnerConfig { FirstObstacleAt = 3f, HighChance = 0f }, 1);
            Assert.That(sim.Finish(), Is.False);
            sim.Step(Frame, true, false);
            Assert.That(sim.Finish(), Is.False);

            Run(sim, 5f);
            Assert.That(sim.Finish(), Is.True);
            Assert.That(sim.Phase, Is.EqualTo(RunnerPhase.Over));
            Assert.That(sim.Finish(), Is.False);
            Assert.That(sim.Continue(), Is.False);
        }
    }
}
