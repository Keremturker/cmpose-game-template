using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class VersionNameTests
    {
        [Test]
        public void Parts_compare_as_numbers_and_a_missing_part_is_zero()
        {
            Assert.That(VersionName.Compare("1.10", "1.9"), Is.GreaterThan(0));
            Assert.That(VersionName.Compare("1.10.0", "1.9.9"), Is.GreaterThan(0));
            Assert.That(VersionName.Compare("1.4.0", "1.4.1"), Is.LessThan(0));
            Assert.That(VersionName.Compare("1.4", "1.4.0"), Is.EqualTo(0));
        }

        [Test]
        public void A_suffix_is_ignored_and_an_empty_version_is_the_oldest()
        {
            Assert.That(VersionName.Compare("2.0.0-beta", "2.0.0"), Is.EqualTo(0));
            Assert.That(VersionName.Compare(" 2.0.1 ", "2.0.0"), Is.GreaterThan(0));
            Assert.That(VersionName.Compare("", "0.0.1"), Is.LessThan(0));
            Assert.That(VersionName.Compare(null, "0.0.1"), Is.LessThan(0));
            Assert.That(VersionName.Compare("1.0.0", ""), Is.GreaterThan(0));
            // a part too large for a long counts as 0
            Assert.That(VersionName.Compare("1.99999999999999999999", "1.0"), Is.EqualTo(0));
        }
    }
}
