namespace Kturker.UnityBase.Core.Domain
{
    // A game the menu offers: its scene and the label key of its Play button. Each game registers one in the root
    // scope; the menu shows them in registration order and the 'game' deep link opens the last one.
    public sealed class GameScene
    {
        public GameScene(SceneId scene, string labelKey)
        {
            Scene = scene;
            LabelKey = labelKey;
        }

        public SceneId Scene { get; }
        public string LabelKey { get; }
    }
}
