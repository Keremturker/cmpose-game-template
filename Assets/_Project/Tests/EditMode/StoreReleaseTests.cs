using Kturker.UnityBase.Core.Update;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class StoreReleaseTests
    {
        [Test]
        public void The_first_result_is_the_store_release()
        {
            // Apple escapes the slashes in its answer
            var release = StoreRelease.Parse(
                "{\"resultCount\":1,\"results\":[{\"version\":\"2.0.1\",\"trackViewUrl\":\"https:\\/\\/apps.apple.com\\/app\\/id1\",\"price\":0}]}");

            Assert.That(release.Version, Is.EqualTo("2.0.1"));
            Assert.That(release.StoreUrl, Is.EqualTo("https://apps.apple.com/app/id1"));
            Assert.That(StoreRelease.LookupUrl("com.example.game"), Is.EqualTo("https://itunes.apple.com/lookup?bundleId=com.example.game"));
        }

        [Test]
        public void No_result_a_broken_answer_or_a_foreign_page_is_no_release()
        {
            Assert.That(StoreRelease.Parse("{\"resultCount\":0,\"results\":[]}"), Is.Null);
            Assert.That(StoreRelease.Parse("{\"resultCount\":1,\"results\":["), Is.Null);
            Assert.That(StoreRelease.Parse("{\"results\":[{\"version\":\"2.0.1\",\"trackViewUrl\":\"http://example.com/app\"}]}"), Is.Null);
            Assert.That(StoreRelease.Parse("{\"results\":[{\"version\":\" \",\"trackViewUrl\":\"https://apps.apple.com/app/id1\"}]}"), Is.Null);
        }
    }
}
