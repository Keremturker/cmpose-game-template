using System.IO;
using Kturker.UnityBase.Editor.Build;
using NUnit.Framework;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class KeystoreConfigTests
    {
        private static readonly string Root = Path.Combine(Path.GetTempPath(), "cmpose-keystore-test");
        private static readonly string Store = Path.GetFullPath(Path.Combine(Root, "Keys/release.keystore"));

        private static bool Exists(string path) => path == Store;

        [Test]
        public void Reads_the_four_fields_skipping_comments_and_resolving_the_store_path()
        {
            const string text = "# release signing\n! also a comment\n\nstoreFile = Keys/release.keystore\n"
                + "storePassword=pa=ss\r\nkeyAlias=release\nkeyPassword= secret \n";

            var config = KeystoreConfig.Parse(text, Root, Exists);

            Assert.That(config.StoreFile, Is.EqualTo(Store));
            Assert.That(config.StorePassword, Is.EqualTo("pa=ss"));
            Assert.That(config.KeyAlias, Is.EqualTo("release"));
            Assert.That(config.KeyPassword, Is.EqualTo("secret"));
        }

        [Test]
        public void Missing_empty_or_malformed_fields_are_bad_input_naming_the_field()
        {
            var missing = Assert.Throws<BuildInputException>(() =>
                KeystoreConfig.Parse("storeFile=Keys/release.keystore\nstorePassword=a\nkeyAlias=b\n", Root, Exists));
            Assert.That(missing.Message, Is.EqualTo("keystore.properties: missing keyPassword"));

            var empty = Assert.Throws<BuildInputException>(() =>
                KeystoreConfig.Parse("storeFile=Keys/release.keystore\nstorePassword=\nkeyAlias=b\nkeyPassword=c\n", Root, Exists));
            Assert.That(empty.Message, Is.EqualTo("keystore.properties: empty storePassword"));

            var malformed = Assert.Throws<BuildInputException>(() =>
                KeystoreConfig.Parse("storeFile Keys/release.keystore\n", Root, Exists));
            Assert.That(malformed.Message, Is.EqualTo("keystore.properties: line 1 has no '='"));
        }

        [Test]
        public void A_store_file_that_does_not_exist_is_bad_input()
        {
            var e = Assert.Throws<BuildInputException>(() =>
                KeystoreConfig.Parse("storeFile=Keys/other.keystore\nstorePassword=a\nkeyAlias=b\nkeyPassword=c\n", Root, Exists));
            Assert.That(e.Message, Is.EqualTo("keystore.properties: storeFile not found: Keys/other.keystore"));
        }
    }
}
