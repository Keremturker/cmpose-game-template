using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class MinVersionsTests
    {
        [Test]
        public void Both_platforms_are_read()
        {
            var min = MinVersions.Parse("{\"minVersion\":{\"android\":12,\"ios\":\"1.4.0\"}}");

            Assert.That(min.Android, Is.EqualTo(12));
            Assert.That(min.Ios, Is.EqualTo("1.4.0"));
        }

        [Test]
        public void A_missing_platform_has_no_minimum()
        {
            var iosOnly = MinVersions.Parse("{\"minVersion\":{\"ios\":\"1.4.0\"},\"note\":\"unknown keys are ignored\"}");
            Assert.That(iosOnly.Android, Is.Null);
            Assert.That(iosOnly.Ios, Is.EqualTo("1.4.0"));

            var androidOnly = MinVersions.Parse(" {\n \"minVersion\" : { \"android\" : 7 , \"ios\" : null } }\n");
            Assert.That(androidOnly.Android, Is.EqualTo(7));
            Assert.That(androidOnly.Ios, Is.Null);
        }

        // A broken file must not force anyone to update, and must not crash the game
        [Test]
        public void Broken_or_unexpected_text_is_no_forced_update()
        {
            Assert.That(MinVersions.Parse(null), Is.Null);
            Assert.That(MinVersions.Parse(""), Is.Null);
            Assert.That(MinVersions.Parse("{\"minVersion\":"), Is.Null);
            Assert.That(MinVersions.Parse("<html><body>404 Not Found</body></html>"), Is.Null);
            Assert.That(MinVersions.Parse("{\"minVersion\":{\"android\":12}} trailing"), Is.Null);
            Assert.That(MinVersions.Parse("{\"minVersion\":\"1.4.0\"}"), Is.Null);
            Assert.That(MinVersions.Parse("{\"android\":12}"), Is.Null);
            Assert.That(MinVersions.Parse("[1,2]"), Is.Null);
            Assert.That(MinVersions.Parse(new string('[', 5000) + new string(']', 5000)), Is.Null);
        }

        [Test]
        public void A_field_of_the_wrong_type_is_no_forced_update()
        {
            Assert.That(MinVersions.Parse("{\"minVersion\":{\"android\":\"12\"}}"), Is.Null);
            Assert.That(MinVersions.Parse("{\"minVersion\":{\"android\":12.5}}"), Is.Null);
            Assert.That(MinVersions.Parse("{\"minVersion\":{\"android\":1e12}}"), Is.Null);
            Assert.That(MinVersions.Parse("{\"minVersion\":{\"ios\":14}}"), Is.Null);
        }
    }
}
