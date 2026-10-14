using Kturker.UnityBase.Core.Domain;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer.Unity;

namespace Kturker.UnityBase.App.Game
{
    // Logs which URP renderer draws the game (UniversalRenderer for 3D, Renderer2D for the 2D layer); the template
    // gate checks it in the 2D variants
    public sealed class GameRenderMarker : IStartable
    {
        private readonly ILog log;

        public GameRenderMarker(ILog log) => this.log = log;

        public void Start() => log.Marker($"CMPOSE_RENDER renderer={RendererName()}");

        public static string RendererName() =>
            GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp && urp.scriptableRenderer != null
                ? urp.scriptableRenderer.GetType().Name
                : "none";
    }
}
