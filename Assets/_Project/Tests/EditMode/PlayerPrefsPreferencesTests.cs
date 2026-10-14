using Kturker.UnityBase.Core.Preferences;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Tests.EditMode
{
    // Uses real editor PlayerPrefs under test-only keys, deleted after each test
    public sealed class PlayerPrefsPreferencesTests
    {
        private const string BoolKey = "cmpose.test.bool";
        private const string IntKey = "cmpose.test.int";
        private const string FloatKey = "cmpose.test.float";
        private const string StringKey = "cmpose.test.string";

        [TearDown]
        public void DeleteTestKeys()
        {
            PlayerPrefs.DeleteKey(BoolKey);
            PlayerPrefs.DeleteKey(IntKey);
            PlayerPrefs.DeleteKey(FloatKey);
            PlayerPrefs.DeleteKey(StringKey);
        }

        [Test]
        public void Missing_keys_return_the_given_defaults()
        {
            var prefs = new PlayerPrefsPreferences();
            Assert.That(prefs.GetBool(BoolKey, true), Is.True);
            Assert.That(prefs.GetBool(BoolKey, false), Is.False);
            Assert.That(prefs.GetInt(IntKey, 7), Is.EqualTo(7));
            Assert.That(prefs.GetFloat(FloatKey, 0.8f), Is.EqualTo(0.8f));
            Assert.That(prefs.GetString(StringKey, "x"), Is.EqualTo("x"));
        }

        [Test]
        public void Values_round_trip_by_type()
        {
            var prefs = new PlayerPrefsPreferences();
            prefs.SetBool(BoolKey, false);
            prefs.SetInt(IntKey, 42);
            prefs.SetFloat(FloatKey, 0.25f);
            prefs.SetString(StringKey, "hello");

            var again = new PlayerPrefsPreferences();
            Assert.That(again.GetBool(BoolKey, true), Is.False);
            Assert.That(again.GetInt(IntKey, 0), Is.EqualTo(42));
            Assert.That(again.GetFloat(FloatKey, 1f), Is.EqualTo(0.25f));
            Assert.That(again.GetString(StringKey, ""), Is.EqualTo("hello"));
        }

        [Test]
        public void A_bool_key_holding_another_int_reads_as_the_default()
        {
            PlayerPrefs.SetInt(BoolKey, 7);
            var prefs = new PlayerPrefsPreferences();
            Assert.That(prefs.GetBool(BoolKey, true), Is.True);
            Assert.That(prefs.GetBool(BoolKey, false), Is.False);
        }
    }
}
