using System.Collections.Generic;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    // A full-screen touch catcher under the scene's UI (buttons and panels above it, the pause panel among them, take
    // their own touches): SwipeTracker turns swipes and taps into track inputs, queued until the presenter takes them,
    // one per frame. It draws nothing; it is a Graphic only so the canvas raycaster finds it.
    public sealed class SwipeArea : Graphic, ITrackInput, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        // A swipe passes this share of the screen's shorter side
        private const float ThresholdShare = 0.06f;
        private const int MaxQueued = 4;

        private readonly Queue<TrackInput> queued = new Queue<TrackInput>();
        private readonly SwipeTracker tracker = new SwipeTracker();

        public TrackInput Consume() => queued.Count > 0 ? queued.Dequeue() : TrackInput.None;

        // What a touch gives; also the PlayMode test's way in
        public void Push(TrackInput input)
        {
            if (input != TrackInput.None && queued.Count < MaxQueued) queued.Enqueue(input);
        }

        public void OnPointerDown(PointerEventData eventData) =>
            tracker.Down(eventData.pointerId, eventData.position.x, eventData.position.y,
                Mathf.Min(Screen.width, Screen.height) * ThresholdShare);

        public void OnDrag(PointerEventData eventData) =>
            Push(tracker.Drag(eventData.pointerId, eventData.position.x, eventData.position.y));

        public void OnPointerUp(PointerEventData eventData) =>
            Push(tracker.Up(eventData.pointerId, eventData.position.x, eventData.position.y));

        protected override void OnDisable()
        {
            base.OnDisable();
            tracker.Cancel();
            queued.Clear();
        }

        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    }
}
