using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UpdateScheduleTests
    {
        private const long Now = 1_800_000_000_000L;
        private const long Hour = 60L * 60 * 1000;

        [Test]
        public void The_first_check_is_due() => Assert.That(UpdateSchedule.IsDue(null, Now, false), Is.True);

        [Test]
        public void Less_than_a_day_later_is_not_due_and_a_day_later_is()
        {
            Assert.That(UpdateSchedule.IsDue(Now - 23 * Hour, Now, false), Is.False);
            Assert.That(UpdateSchedule.IsDue(Now - 24 * Hour, Now, false), Is.True);
        }

        // Reopening the game must not get past a required update; a clock set back checks instead of waiting
        [Test]
        public void A_required_update_is_due_every_time_and_a_clock_set_back_is_due()
        {
            Assert.That(UpdateSchedule.IsDue(Now - Hour, Now, true), Is.True);
            Assert.That(UpdateSchedule.IsDue(Now + Hour, Now, false), Is.True);
        }
    }
}
