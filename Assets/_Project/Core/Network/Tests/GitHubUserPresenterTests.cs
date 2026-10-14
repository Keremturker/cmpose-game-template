using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Features.Sample.Network;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Core.Network.Tests
{
    public sealed class GitHubUserPresenterTests
    {
        private const string AvatarUrl = "https://avatars.example.test/u/583231";
        private readonly List<Texture2D> made = new List<Texture2D>();

        [TearDown]
        public void DestroyTextures()
        {
            foreach (var texture in made)
            {
                if (texture != null) Object.DestroyImmediate(texture);
            }
            made.Clear();
        }

        private static ApiResult<GitHubUser> User(string name = "The Octocat", string avatar = AvatarUrl) =>
            ApiResult<GitHubUser>.Success(new GitHubUser { Login = "octocat", Name = name, AvatarUrl = avatar });

        private static ApiResult<GitHubUser> Fail(NetworkErrorKind kind) =>
            ApiResult<GitHubUser>.Failure(kind == NetworkErrorKind.Http ? NetworkError.Http(403, "{}") : new NetworkError(kind));

        private GitHubUserPresenter Make(out FakeGitHubUserCardView view, out FakeApiClient api, out FakeImageLoader images, out FakeLog log)
        {
            view = new FakeGitHubUserCardView();
            api = new FakeApiClient();
            var texture = new Texture2D(1, 1);
            made.Add(texture);
            images = new FakeImageLoader { Texture = texture };
            log = new FakeLog();
            return new GitHubUserPresenter(view, api, images, log);
        }

        [Test]
        public void Start_shows_loading_then_the_user_and_the_avatar_and_logs_ok()
        {
            var presenter = Make(out var view, out var api, out var images, out var log);
            api.Results.Enqueue(User());
            presenter.Start();

            Assert.That(api.Paths, Is.EqualTo(new[] { "users/octocat" }));
            Assert.That(view.Calls, Is.EqualTo(new[] { "loading", "user The Octocat octocat", "avatar" }));
            Assert.That(view.Avatar, Is.SameAs(images.Texture));
            Assert.That(images.Urls, Is.EqualTo(new[] { AvatarUrl }));
            Assert.That(log.Lines, Has.Some.EqualTo("MARKER CMPOSE_NET user=octocat status=ok"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void A_user_without_a_name_shows_the_login(string name)
        {
            var presenter = Make(out var view, out var api, out _, out _);
            api.Results.Enqueue(User(name));
            presenter.Start();

            Assert.That(view.Calls, Has.Some.EqualTo("user octocat octocat"));
        }

        [Test]
        public void A_missing_avatar_keeps_the_placeholder()
        {
            var presenter = Make(out var view, out var api, out _, out _);
            api.Results.Enqueue(User(avatar: null));
            presenter.Start();

            Assert.That(view.Calls, Is.EqualTo(new[] { "loading", "user The Octocat octocat" }));
        }

        [TestCase(NetworkErrorKind.NoConnection, "network.error.noConnection 0")]
        [TestCase(NetworkErrorKind.Timeout, "network.error.timeout 0")]
        [TestCase(NetworkErrorKind.Http, "network.error.http 403")]
        [TestCase(NetworkErrorKind.Parse, "network.error.parse 0")]
        public void An_error_shows_its_text_with_retry_and_logs_the_kind(NetworkErrorKind kind, string shown)
        {
            var presenter = Make(out var view, out var api, out _, out var log);
            api.Results.Enqueue(Fail(kind));
            presenter.Start();

            Assert.That(view.Calls, Is.EqualTo(new[] { "loading", $"error {shown}" }));
            Assert.That(log.Lines, Has.Some.EqualTo($"MARKER CMPOSE_NET user=octocat status=error:{kind}"));
        }

        [Test]
        public void A_cancelled_call_shows_nothing_more()
        {
            var presenter = Make(out var view, out var api, out _, out var log);
            api.Results.Enqueue(Fail(NetworkErrorKind.Cancelled));
            presenter.Start();

            Assert.That(view.Calls, Is.EqualTo(new[] { "loading" }));
            Assert.That(log.Lines, Has.None.Contains("CMPOSE_NET"));
        }

        [Test]
        public void Retry_loads_again_and_can_succeed()
        {
            var presenter = Make(out var view, out var api, out _, out _);
            api.Results.Enqueue(Fail(NetworkErrorKind.NoConnection));
            api.Results.Enqueue(User());
            presenter.Start();
            view.Retry();

            Assert.That(api.Paths.Count, Is.EqualTo(2));
            Assert.That(view.Calls, Is.EqualTo(new[]
            {
                "loading", "error network.error.noConnection 0", "loading", "user The Octocat octocat", "avatar"
            }));
        }

        [Test]
        public void Retry_while_loading_is_ignored()
        {
            var presenter = Make(out var view, out var api, out _, out _);
            api.Pending = new UniTaskCompletionSource<object>();
            presenter.Start();
            view.Retry();

            Assert.That(api.Paths.Count, Is.EqualTo(1));
            Assert.That(view.Calls, Is.EqualTo(new[] { "loading" }));
        }

        [Test]
        public void After_dispose_a_late_answer_changes_nothing()
        {
            var presenter = Make(out var view, out var api, out _, out var log);
            var pending = new UniTaskCompletionSource<object>();
            api.Pending = pending;
            presenter.Start();
            presenter.Dispose();
            pending.TrySetResult(User());

            Assert.That(view.Calls, Is.EqualTo(new[] { "loading" }));
            Assert.That(log.Lines, Has.None.Contains("CMPOSE_NET"));
        }
    }
}
