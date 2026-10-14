using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Network.Tests
{
    public sealed class NetworkKeysTests
    {
        [Test]
        public void Every_error_kind_and_the_card_keys_have_an_english_text()
        {
            var english = new TableLocalizer(new ResourcesStringSource(), new FakeLog(), false);
            foreach (NetworkErrorKind kind in Enum.GetValues(typeof(NetworkErrorKind)))
            {
                var key = NetworkKeys.Error(kind);
                Assert.That(english.Get(key), Is.Not.EqualTo(key), key);
            }
            Assert.That(english.Get(NetworkKeys.Loading), Is.Not.EqualTo(NetworkKeys.Loading));
            Assert.That(english.Get(NetworkKeys.Retry), Is.Not.EqualTo(NetworkKeys.Retry));
            Assert.That(english.Get(NetworkKeys.Error(NetworkErrorKind.Http), 404), Does.Contain("404"));
        }
    }
}
