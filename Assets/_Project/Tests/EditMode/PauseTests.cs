using Kturker.UnityBase.Core.Lifecycle;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class PauseTests
    {
        [Test]
        public void Two_reasons_stack_and_removing_one_keeps_the_game_paused()
        {
            var pause = new PauseService();
            pause.Pause(PauseReason.User);
            pause.Pause(PauseReason.Focus);

            pause.Resume(PauseReason.User);

            Assert.That(pause.IsPaused, Is.True);
            Assert.That(pause.Has(PauseReason.User), Is.False);
            Assert.That(pause.Has(PauseReason.Focus), Is.True);

            pause.Resume(PauseReason.Focus);
            Assert.That(pause.IsPaused, Is.False);
        }

        [Test]
        public void Events_fire_only_on_transitions()
        {
            var pause = new PauseService();
            var paused = 0;
            var resumed = 0;
            pause.Paused += () => paused++;
            pause.Resumed += () => resumed++;

            pause.Resume(PauseReason.User);
            pause.Pause(PauseReason.User);
            pause.Pause(PauseReason.User);
            pause.Pause(PauseReason.Focus);
            pause.Resume(PauseReason.Focus);
            pause.Resume(PauseReason.User);
            pause.Resume(PauseReason.User);

            Assert.That(paused, Is.EqualTo(1));
            Assert.That(resumed, Is.EqualTo(1));
        }

        [Test]
        public void Effects_freeze_time_and_music_while_paused()
        {
            var pause = new PauseService();
            var time = new FakeTimeScale();
            var audio = new FakeAudioPlayer();
            var effects = new PauseEffects(pause, time, audio);
            effects.Start();

            pause.Pause(PauseReason.User);
            Assert.That(time.Scale, Is.EqualTo(0f));
            Assert.That(audio.MusicPaused, Is.True);

            pause.Resume(PauseReason.User);
            Assert.That(time.Scale, Is.EqualTo(1f));
            Assert.That(audio.MusicPaused, Is.False);

            effects.Dispose();
            pause.Pause(PauseReason.User);
            Assert.That(time.Scale, Is.EqualTo(1f));
        }

        [Test]
        public void Focus_loss_during_a_user_pause_restores_the_original_time_scale_once_both_end()
        {
            var pause = new PauseService();
            var time = new FakeTimeScale { Scale = 1.5f };
            var audio = new FakeAudioPlayer();
            new PauseEffects(pause, time, audio).Start();

            pause.Pause(PauseReason.User);
            pause.Pause(PauseReason.Focus);
            pause.Resume(PauseReason.User);
            Assert.That(time.Scale, Is.EqualTo(0f));

            pause.Resume(PauseReason.Focus);
            Assert.That(time.Scale, Is.EqualTo(1.5f));
            Assert.That(audio.MusicPauses, Is.EqualTo(1));
            Assert.That(audio.MusicResumes, Is.EqualTo(1));
        }
    }
}
