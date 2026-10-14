using System.Collections.Generic;
using System.Linq;
using Kturker.UnityBase.Core.Network;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.NetworkInspector.Tests
{
    public sealed class InspectorObserverTests
    {
        private static HttpRequest Get(string url, Dictionary<string, string> headers = null) => new HttpRequest("GET", url, headers);

        private static HttpResponse Ok(string body = "{}", double ms = 312.4) =>
            new HttpResponse(TransportFailure.None, 200, body, new Dictionary<string, string> { { "Content-Type", "application/json" } }, ms);

        [Test]
        public void Keeps_the_newest_first_up_to_the_capacity()
        {
            var observer = new InspectorObserver();
            for (var i = 0; i < 55; i++) observer.OnCompleted(Get($"https://api.example.test/n/{i}"), Ok());

            Assert.That(observer.Entries.Count, Is.EqualTo(InspectorObserver.Capacity));
            Assert.That(observer.Entries.First().Summary, Does.EndWith("/n/54"));
            Assert.That(observer.Entries.Last().Summary, Does.EndWith("/n/5"));
        }

        [Test]
        public void The_summary_shows_method_status_time_and_path_and_a_failure_by_name()
        {
            var ok = InspectorEntry.From(Get("https://api.example.test/users/octocat?x=1"), Ok());
            var timeout = InspectorEntry.From(Get("https://api.example.test/users/octocat"),
                new HttpResponse(TransportFailure.Timeout, 0, null, null, 15000));

            Assert.That(ok.Summary, Is.EqualTo("GET 200 312ms /users/octocat?x=1"));
            Assert.That(timeout.Summary, Is.EqualTo("GET Timeout 15000ms /users/octocat"));
            Assert.That(timeout.Detail, Does.Contain("Status: Timeout"));
        }

        [Test]
        public void Authorization_is_masked_whatever_its_case()
        {
            var entry = InspectorEntry.From(
                Get("https://api.example.test/me", new Dictionary<string, string> { { "authorization", "Bearer abc123" }, { "Accept", "application/json" } }),
                Ok());

            Assert.That(entry.Detail, Does.Contain("authorization: " + InspectorEntry.Masked));
            Assert.That(entry.Detail, Does.Contain("Accept: application/json"));
            Assert.That(entry.Detail, Does.Not.Contain("abc123"));
        }

        [Test]
        public void Long_bodies_are_cut_at_4_KB()
        {
            var entry = InspectorEntry.From(Get("https://api.example.test/big"), Ok(new string('y', 5000)));

            Assert.That(entry.Detail, Does.Contain(new string('y', InspectorEntry.BodyLimit)));
            Assert.That(entry.Detail, Does.Not.Contain(new string('y', InspectorEntry.BodyLimit + 1)));
            Assert.That(entry.Detail, Does.Contain("(truncated)"));
        }

        [Test]
        public void Clear_empties_the_list_and_notifies_only_when_something_was_there()
        {
            var observer = new InspectorObserver();
            var changes = 0;
            observer.Changed += () => changes++;
            observer.Clear();
            observer.OnCompleted(Get("https://api.example.test/a"), Ok());
            observer.Clear();

            Assert.That(observer.Entries, Is.Empty);
            Assert.That(changes, Is.EqualTo(2));
        }
    }
}
