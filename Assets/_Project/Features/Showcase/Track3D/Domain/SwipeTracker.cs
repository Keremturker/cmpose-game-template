using System;

namespace Kturker.UnityBase.Features.Showcase.Track3D.Domain
{
    // One touch at a time on the swipe area (other fingers are ignored until it lifts). A drag that passes the
    // threshold on its main axis reports its direction once: left, right, up = jump, down = nothing. A touch that lifts
    // without passing it is a tap: jump. Positions are screen pixels, y up.
    public sealed class SwipeTracker
    {
        private bool active;
        private bool reported;
        private int pointer;
        private float startX;
        private float startY;
        private float threshold;

        public void Down(int pointerId, float x, float y, float thresholdPixels)
        {
            if (active) return;
            active = true;
            reported = false;
            pointer = pointerId;
            startX = x;
            startY = y;
            threshold = thresholdPixels;
        }

        public TrackInput Drag(int pointerId, float x, float y)
        {
            if (!active || reported || pointerId != pointer || !Passed(x, y)) return TrackInput.None;
            reported = true;
            return Direction(x - startX, y - startY);
        }

        public TrackInput Up(int pointerId, float x, float y)
        {
            if (!active || pointerId != pointer) return TrackInput.None;
            active = false;
            if (reported) return TrackInput.None;
            return Passed(x, y) ? Direction(x - startX, y - startY) : TrackInput.Jump;
        }

        public void Cancel() => active = false;

        private bool Passed(float x, float y) => Math.Max(Math.Abs(x - startX), Math.Abs(y - startY)) >= threshold;

        private static TrackInput Direction(float dx, float dy)
        {
            if (Math.Abs(dx) >= Math.Abs(dy)) return dx < 0f ? TrackInput.Left : TrackInput.Right;
            return dy > 0f ? TrackInput.Jump : TrackInput.None;
        }
    }
}
