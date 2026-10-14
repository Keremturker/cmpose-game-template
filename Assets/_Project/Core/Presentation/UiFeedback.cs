using Kturker.UnityBase.Core.Audio;
using Kturker.UnityBase.Core.Haptics;

namespace Kturker.UnityBase.Core.Presentation
{
    public sealed class UiFeedback : IUiFeedback
    {
        private readonly IAudioPlayer audio;
        private readonly UiSounds sounds;
        private readonly HapticsService haptics;

        public UiFeedback(IAudioPlayer audio, UiSounds sounds, HapticsService haptics)
        {
            this.audio = audio;
            this.sounds = sounds;
            this.haptics = haptics;
        }

        public void Click()
        {
            audio.PlaySfx(sounds.Click);
            haptics.Play(HapticStrength.Light);
        }
    }
}
