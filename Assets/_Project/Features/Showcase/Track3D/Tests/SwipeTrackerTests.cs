using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using NUnit.Framework;

namespace Kturker.UnityBase.Features.Showcase.Track3D.Tests
{
    public sealed class SwipeTrackerTests
    {
        private const float Threshold = 60f;

        // One touch from (x0, y0) to (x1, y1): what the drag reported, or else what the release did
        private static TrackInput Swipe(float x0, float y0, float x1, float y1)
        {
            var tracker = new SwipeTracker();
            tracker.Down(0, x0, y0, Threshold);
            var dragged = tracker.Drag(0, x1, y1);
            var released = tracker.Up(0, x1, y1);
            return dragged != TrackInput.None ? dragged : released;
        }

        [Test]
        public void A_short_touch_is_a_tap_that_jumps_even_when_it_wobbles()
        {
            var tracker = new SwipeTracker();
            tracker.Down(0, 500f, 900f, Threshold);

            Assert.That(tracker.Drag(0, 540f, 870f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Up(0, 530f, 880f), Is.EqualTo(TrackInput.Jump));
        }

        [Test]
        public void A_drag_past_the_threshold_reports_its_main_direction_once_however_far_it_goes()
        {
            var tracker = new SwipeTracker();
            tracker.Down(0, 500f, 900f, Threshold);

            Assert.That(tracker.Drag(0, 450f, 905f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Drag(0, 430f, 920f), Is.EqualTo(TrackInput.Left));
            Assert.That(tracker.Drag(0, 100f, 900f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Drag(0, 900f, 900f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Up(0, 900f, 900f), Is.EqualTo(TrackInput.None));
        }

        [Test]
        public void Right_and_up_swipes_move_right_and_jump_and_a_down_swipe_does_nothing()
        {
            Assert.That(Swipe(500f, 900f, 600f, 940f), Is.EqualTo(TrackInput.Right));
            Assert.That(Swipe(500f, 900f, 520f, 1000f), Is.EqualTo(TrackInput.Jump));
            Assert.That(Swipe(500f, 900f, 510f, 780f), Is.EqualTo(TrackInput.None));
        }

        [Test]
        public void A_fast_swipe_seen_only_at_the_release_still_counts()
        {
            var tracker = new SwipeTracker();
            tracker.Down(0, 500f, 900f, Threshold);

            Assert.That(tracker.Up(0, 380f, 910f), Is.EqualTo(TrackInput.Left));
        }

        [Test]
        public void A_second_finger_is_ignored_until_the_first_lifts()
        {
            var tracker = new SwipeTracker();
            tracker.Down(0, 500f, 900f, Threshold);
            tracker.Down(1, 200f, 400f, Threshold);

            Assert.That(tracker.Drag(1, 400f, 400f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Up(1, 400f, 400f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Up(0, 505f, 900f), Is.EqualTo(TrackInput.Jump));

            tracker.Down(1, 200f, 400f, Threshold);
            Assert.That(tracker.Up(1, 400f, 400f), Is.EqualTo(TrackInput.Right));
        }

        [Test]
        public void Cancel_forgets_the_touch()
        {
            var tracker = new SwipeTracker();
            tracker.Down(0, 500f, 900f, Threshold);
            tracker.Cancel();

            Assert.That(tracker.Drag(0, 300f, 900f), Is.EqualTo(TrackInput.None));
            Assert.That(tracker.Up(0, 300f, 900f), Is.EqualTo(TrackInput.None));
        }
    }
}
