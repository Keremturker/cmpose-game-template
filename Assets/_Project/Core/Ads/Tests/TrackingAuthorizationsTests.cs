using System.Linq;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Ads.Tests
{
    public sealed class TrackingAuthorizationsTests
    {
        [Test]
        public void Att_codes_map_to_statuses_and_the_editor_has_no_prompt()
        {
            Assert.That(new[] { 0, 1, 2, 3, 9 }.Select(TrackingAuthorizations.FromAtt), Is.EqualTo(new[]
            {
                TrackingStatus.NotDetermined, TrackingStatus.Restricted, TrackingStatus.Denied, TrackingStatus.Authorized,
                TrackingStatus.NotDetermined
            }));
            var editor = TrackingAuthorizations.ForPlatform();
            Assert.That(editor, Is.InstanceOf<NoTrackingAuthorization>());
            Assert.That(editor.Status, Is.EqualTo(TrackingStatus.NotApplicable));
            Assert.That(editor.RequestAsync().GetAwaiter().GetResult(), Is.EqualTo(TrackingStatus.NotApplicable));
        }
    }
}
