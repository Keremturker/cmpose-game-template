using VContainer.Unity;

namespace Kturker.UnityBase.Core.Presentation
{
    // Registered by every scene scope: binds the scene's texts (and colours) once the scope starts
    public sealed class SceneUiBinding : IStartable
    {
        private readonly UiBinder binder;

        public SceneUiBinding(UiBinder binder) => this.binder = binder;

        public void Start() => binder.BindScene();
    }
}
