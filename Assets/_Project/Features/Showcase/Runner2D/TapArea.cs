using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    // A full-screen touch catcher under the scene's UI (buttons above it take their own touches). It draws nothing;
    // it is a Graphic only so the canvas raycaster finds it.
    public sealed class TapArea : Graphic, ITapInput, IPointerDownHandler, IPointerUpHandler
    {
        private bool pressed;
        private int down;

        public bool Held => down > 0;

        public bool ConsumePress()
        {
            var was = pressed;
            pressed = false;
            return was;
        }

        public void Press()
        {
            pressed = true;
            down++;
        }

        public void Release()
        {
            if (down > 0) down--;
        }

        public void OnPointerDown(PointerEventData eventData) => Press();

        public void OnPointerUp(PointerEventData eventData) => Release();

        protected override void OnDisable()
        {
            base.OnDisable();
            pressed = false;
            down = 0;
        }

        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    }
}
