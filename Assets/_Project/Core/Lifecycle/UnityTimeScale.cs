using UnityEngine;

namespace Kturker.UnityBase.Core.Lifecycle
{
    public sealed class UnityTimeScale : ITimeScale
    {
        public float Scale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }
    }
}
