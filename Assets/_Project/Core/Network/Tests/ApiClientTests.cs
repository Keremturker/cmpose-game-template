using System.Linq;
using System.Threading;
using Kturker.UnityBase.Tests.EditMode;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Network.Tests
{
    public sealed class ApiClientTests
    {
        private const string Base = "https://api.example.test/";

        public sealed class Item
        {
            [JsonProperty("display_name")] public string DisplayName;
            [JsonProperty("count")] public int Count;
        }

        private static ApiClient Make(HttpResponse response, out FakeTransport transport, out FakeLog log, bool offline = false,
            params IRequestObserver[] observers)
        {
            transport = new FakeTransport { Response = response };
            log = new FakeLog();
            return new ApiClient(transport, Base, observers, new FakeReachability { IsOffline = offline }, log);
        }

        private static ApiResult<Item> Get(ApiClient client, string path, CancellationToken cancellation = default) =>
            client.GetJson<Item>(path, cancellation).GetAwaiter().GetResult();

        [Test]
        public void A_2xx_json_answer_is_parsed_and_the_request_asks_for_json()
        {
            var client = Make(FakeTransport.Ok("{\"display_name\":\"Octo\",\"count\":3,\"extra\":true}"), out var transport, out _);
            var result = Get(client, "users/octocat");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.DisplayName, Is.EqualTo("Octo"));
            Assert.That(result.Value.Count, Is.EqualTo(3));
            var request = transport.Requests.Single();
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.Url, Is.EqualTo("https://api.example.test/users/octocat"));
            Assert.That(request.Headers["Accept"], Is.EqualTo("application/json"));
            Assert.That(transport.Timeouts.Single(), Is.EqualTo(ApiConfig.Timeout));
        }

        [TestCase("https://api.example.test/", "users/octocat")]
        [TestCase("https://api.example.test", "/users/octocat")]
        [TestCase("https://api.example.test//", "//users/octocat")]
        public void Base_url_and_path_join_with_one_slash(string baseUrl, string path)
        {
            Assert.That(ApiClient.Combine(baseUrl, path), Is.EqualTo("https://api.example.test/users/octocat"));
        }

        [TestCase(TransportFailure.Connection, NetworkErrorKind.NoConnection)]
        [TestCase(TransportFailure.Timeout, NetworkErrorKind.Timeout)]
        [TestCase(TransportFailure.Cancelled, NetworkErrorKind.Cancelled)]
        public void Transport_failures_become_typed_errors(TransportFailure failure, NetworkErrorKind expected)
        {
            var result = Get(Make(FakeTransport.Failed(failure), out _, out _), "users/octocat");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Kind, Is.EqualTo(expected));
        }

        [Test]
        public void A_non_2xx_answer_is_an_http_error_with_the_code_and_the_start_of_the_body()
        {
            var error = Get(Make(FakeTransport.Status(403, new string('x', 2000)), out _, out _), "users/octocat").Error;

            Assert.That(error.Kind, Is.EqualTo(NetworkErrorKind.Http));
            Assert.That(error.StatusCode, Is.EqualTo(403));
            Assert.That(error.Body, Is.EqualTo(new string('x', NetworkError.BodyLimit)));
        }

        [TestCase("<html>rate limited</html>")]
        [TestCase("")]
        [TestCase("null")]
        [TestCase("{\"count\":\"many\"}")]
        public void An_unreadable_answer_is_a_parse_error_and_logged_without_throwing(string body)
        {
            var client = Make(FakeTransport.Ok(body), out _, out var log);

            Assert.That(Get(client, "users/octocat").Error.Kind, Is.EqualTo(NetworkErrorKind.Parse));
            Assert.That(log.Lines, Has.Some.StartsWith("WARNING"));
        }

        [Test]
        public void Offline_answers_no_connection_without_a_request()
        {
            var client = Make(FakeTransport.Ok("{}"), out var transport, out _, offline: true);

            Assert.That(Get(client, "users/octocat").Error.Kind, Is.EqualTo(NetworkErrorKind.NoConnection));
            Assert.That(transport.Requests, Is.Empty);
        }

        [Test]
        public void An_already_cancelled_call_sends_nothing()
        {
            var client = Make(FakeTransport.Ok("{}"), out var transport, out _);
            using var source = new CancellationTokenSource();
            source.Cancel();

            Assert.That(Get(client, "users/octocat", source.Token).Error.Kind, Is.EqualTo(NetworkErrorKind.Cancelled));
            Assert.That(transport.Requests, Is.Empty);
        }

        [Test]
        public void Every_observer_sees_the_exchange_and_a_failing_one_is_only_logged()
        {
            var seen = new RecordingObserver();
            var client = Make(FakeTransport.Ok("{\"count\":1}"), out _, out var log, false, new ThrowingObserver(), seen);

            Assert.That(Get(client, "users/octocat").IsSuccess, Is.True);
            Assert.That(seen.Requests.Single().Url, Is.EqualTo("https://api.example.test/users/octocat"));
            Assert.That(seen.Responses.Single().StatusCode, Is.EqualTo(200));
            Assert.That(log.Lines, Has.Some.Contains("observer"));
        }

        [Test]
        public void The_log_has_the_path_and_status_but_no_query_or_headers()
        {
            var client = Make(FakeTransport.Ok("{\"count\":1}"), out _, out var log);
            Get(client, "/search/users?q=secret-token");

            Assert.That(log.Lines, Has.Some.Contains("GET search/users -> 200"));
            Assert.That(log.Lines.Any(l => l.Contains("secret-token") || l.Contains("application/json")), Is.False);
        }
    }
}
