using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Navigation;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class BootEntryTests
    {
        [UnityTest]
        public IEnumerator Runs_steps_in_order_then_opens_the_menu() => UniTask.ToCoroutine(async () =>
        {
            var order = new List<string>();
            var loader = new FakeSceneLoader();
            var entry = new BootEntry(new IBootStep[] { new FakeStep("a", order), new FakeStep("b", order) },
                loader, new FakeBootErrorView(), new FakeLog());

            await entry.StartAsync(CancellationToken.None);

            Assert.That(order, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
        });

        [UnityTest]
        public IEnumerator Without_steps_it_opens_the_menu() => UniTask.ToCoroutine(async () =>
        {
            var loader = new FakeSceneLoader();
            await new BootEntry(new IBootStep[0], loader, new FakeBootErrorView(), new FakeLog()).StartAsync(CancellationToken.None);
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
        });

        [UnityTest]
        public IEnumerator A_failed_step_shows_the_error_and_retry_resumes_from_it() => UniTask.ToCoroutine(async () =>
        {
            var order = new List<string>();
            var a = new FakeStep("a", order);
            var b = new FakeStep("b", order, failures: 1);
            var c = new FakeStep("c", order);
            var loader = new FakeSceneLoader();
            var errorView = new FakeBootErrorView();
            var log = new FakeLog();
            var entry = new BootEntry(new IBootStep[] { a, b, c }, loader, errorView, log);

            await entry.StartAsync(CancellationToken.None);

            Assert.That(order, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(loader.Requested, Is.Empty);
            Assert.That(errorView.Shown, Is.EqualTo(1));
            Assert.That(errorView.Message, Is.EqualTo(BootEntry.StepFailedKey));
            Assert.That(errorView.Detail, Is.EqualTo("b"));
            Assert.That(log.Lines, Does.Contain("ERROR Boot step failed: b"));

            // The fake steps finish synchronously, so the retried run completes inside this call
            errorView.Retry();

            Assert.That(order, Is.EqualTo(new[] { "a", "b", "b", "c" }));
            Assert.That(a.Runs, Is.EqualTo(1));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
        });

        [UnityTest]
        public IEnumerator A_second_retry_while_a_run_is_active_is_ignored() => UniTask.ToCoroutine(async () =>
        {
            var order = new List<string>();
            var b = new FakeStep("b", order, failures: 1) { Gate = new UniTaskCompletionSource() };
            var c = new FakeStep("c", order);
            var loader = new FakeSceneLoader();
            var errorView = new FakeBootErrorView();
            var log = new FakeLog();
            var entry = new BootEntry(new IBootStep[] { b, c }, loader, errorView, log);

            await entry.StartAsync(CancellationToken.None);
            errorView.Retry();
            errorView.Retry();

            Assert.That(b.Runs, Is.EqualTo(2));
            Assert.That(log.Lines, Does.Contain("WARNING Boot retry ignored while a run is active"));

            b.Gate.TrySetResult();

            Assert.That(b.Runs, Is.EqualTo(2));
            Assert.That(c.Runs, Is.EqualTo(1));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
        });

        [UnityTest]
        public IEnumerator A_cancelled_step_ends_boot_without_the_error_view() => UniTask.ToCoroutine(async () =>
        {
            var order = new List<string>();
            var loader = new FakeSceneLoader();
            var errorView = new FakeBootErrorView();
            var log = new FakeLog();
            var entry = new BootEntry(new IBootStep[] { new FakeStep("a", order) { CancelOnRun = true }, new FakeStep("b", order) },
                loader, errorView, log);

            await entry.StartAsync(CancellationToken.None);

            Assert.That(order, Is.EqualTo(new[] { "a" }));
            Assert.That(errorView.Shown, Is.EqualTo(0));
            Assert.That(loader.Requested, Is.Empty);
            Assert.That(log.Lines, Does.Contain("INFO Boot cancelled during: a"));
        });

        [UnityTest]
        public IEnumerator A_step_that_faults_after_awaiting_shows_the_error_and_retry_finishes() => UniTask.ToCoroutine(async () =>
        {
            var order = new List<string>();
            var b = new FakeStep("b", order) { Gate = new UniTaskCompletionSource(), FailAfterGate = true };
            var loader = new FakeSceneLoader();
            var errorView = new FakeBootErrorView();
            var entry = new BootEntry(new IBootStep[] { b }, loader, errorView, new FakeLog());

            var run = entry.StartAsync(CancellationToken.None);
            Assert.That(errorView.Shown, Is.EqualTo(0));
            b.Gate.TrySetResult();
            await run;

            Assert.That(errorView.Shown, Is.EqualTo(1));
            Assert.That(errorView.Message, Is.EqualTo(BootEntry.StepFailedKey));
            Assert.That(errorView.Detail, Is.EqualTo("b"));

            errorView.Retry();

            Assert.That(b.Runs, Is.EqualTo(2));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu }));
        });

        [UnityTest]
        public IEnumerator A_menu_that_fails_to_open_shows_the_error_again_after_retry() => UniTask.ToCoroutine(async () =>
        {
            var loader = new FakeSceneLoader { Failures = 2 };
            var errorView = new FakeBootErrorView();
            var log = new FakeLog();
            var entry = new BootEntry(new IBootStep[0], loader, errorView, log);

            await entry.StartAsync(CancellationToken.None);
            Assert.That(errorView.Shown, Is.EqualTo(1));
            Assert.That(errorView.Message, Is.EqualTo(BootEntry.MenuFailedKey));
            Assert.That(errorView.Detail, Is.Null);
            Assert.That(log.Lines, Does.Contain("ERROR Menu failed to open"));

            errorView.Retry();
            Assert.That(errorView.Shown, Is.EqualTo(2));

            errorView.Retry();
            Assert.That(errorView.Shown, Is.EqualTo(2));
            Assert.That(loader.Requested, Is.EqualTo(new[] { SceneId.Menu, SceneId.Menu, SceneId.Menu }));
        });
    }
}
