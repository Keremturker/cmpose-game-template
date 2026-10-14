using UnityEngine;

namespace Kturker.UnityBase.Core.Audio
{
    // Clips every screen shares; set on the root scope prefab
    public sealed class UiSounds
    {
        public UiSounds(AudioClip click) => Click = click;

        public AudioClip Click { get; }
    }
}
