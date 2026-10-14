using UnityEngine.UI;

namespace Kturker.UnityBase.Core.Presentation
{
    // Draws nothing; a Graphic only so the canvas raycaster finds the area (the module scroll's viewport, so a drag on
    // an empty part of a module panel still scrolls)
    public sealed class RaycastSurface : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    }
}
