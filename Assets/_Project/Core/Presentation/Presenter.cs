using System;
using VContainer.Unity;

namespace Kturker.UnityBase.Core.Presentation
{
    // MVP presenter: plain C#, started by the scope; views are MonoBehaviours behind an interface
    public abstract class Presenter<TView> : IStartable, IDisposable where TView : class
    {
        protected Presenter(TView view) => View = view ?? throw new ArgumentNullException(nameof(view));

        protected TView View { get; }

        public abstract void Start();

        public virtual void Dispose()
        {
        }
    }
}
