using System.Collections;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.App.Menu;
using Kturker.UnityBase.Core.Localization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
// cmpose:sample
using Kturker.UnityBase.Features.Sample.Presentation;
// /cmpose:sample
// cmpose:showcase
using Kturker.UnityBase.Features.Showcase.Common;
// /cmpose:showcase
// cmpose:runner2D
using Kturker.UnityBase.Features.Showcase.Runner2D;
// /cmpose:runner2D
// cmpose:track3D
using Kturker.UnityBase.Features.Showcase.Track3D;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
// /cmpose:track3D

namespace Kturker.UnityBase.Tests.PlayMode
{
    public sealed class BootFlowTests
    {
        // cmpose:sample
        [UnityTest]
        public IEnumerator Boot_opens_the_menu_and_play_opens_the_game() => UniTask.ToCoroutine(async () =>
        {
            await SceneManager.LoadSceneAsync("Boot").ToUniTask();

            var menu = await WaitFor<MenuView>(10f);
            Assert.That(menu, Is.Not.Null, "Menu did not open within 10 s");

            GameButton(menu, "menu.play").onClick.Invoke();
            var sample = await WaitFor<SampleView>(10f);
            Assert.That(sample, Is.Not.Null, "Game did not open within 10 s");
            Assert.That(sample.isActiveAndEnabled, Is.True);
        });
        // /cmpose:sample

        // cmpose:runner2D
        [UnityTest]
        public IEnumerator The_runner_starts_on_a_touch_and_its_crash_opens_the_game_over_panel() => UniTask.ToCoroutine(async () =>
        {
            await SceneManager.LoadSceneAsync("Boot").ToUniTask();
            var menu = await WaitFor<MenuView>(10f);
            Assert.That(menu, Is.Not.Null, "Menu did not open within 10 s");

            GameButton(menu, "runner2d.play").onClick.Invoke();
            var tap = await WaitFor<TapArea>(10f);
            Assert.That(tap, Is.Not.Null, "Runner2D did not open within 10 s");
            await UniTask.DelayFrame(2);
            var runner = GameObject.Find("Runner");
            Assert.That(runner, Is.Not.Null);
            var onScreen = Camera.main.WorldToViewportPoint(runner.transform.position);
            Assert.That(onScreen.x > 0f && onScreen.x < 1f && onScreen.y > 0f && onScreen.y < 1f, Is.True, $"runner at {onScreen}");

            tap.Press();
            tap.Release();
            var gameOver = Object.FindFirstObjectByType<GameOverView>();
            var until = Time.realtimeSinceStartup + 30f;
            while (!gameOver.IsShown && Time.realtimeSinceStartup < until) await UniTask.Yield();
            Assert.That(gameOver.IsShown, Is.True, "the runner did not crash into an obstacle within 30 s");
        });
        // /cmpose:runner2D

        // cmpose:track3D
        [UnityTest]
        public IEnumerator The_track_starts_on_a_touch_and_its_crash_opens_the_game_over_panel() => UniTask.ToCoroutine(async () =>
        {
            await SceneManager.LoadSceneAsync("Boot").ToUniTask();
            var menu = await WaitFor<MenuView>(10f);
            Assert.That(menu, Is.Not.Null, "Menu did not open within 10 s");

            GameButton(menu, "track3d.play").onClick.Invoke();
            var swipe = await WaitFor<SwipeArea>(10f);
            Assert.That(swipe, Is.Not.Null, "Track3D did not open within 10 s");
            await UniTask.DelayFrame(2);
            var ball = GameObject.Find("Ball");
            Assert.That(ball, Is.Not.Null);
            var onScreen = Camera.main.WorldToViewportPoint(ball.transform.position);
            Assert.That(onScreen.z > 0f && onScreen.x > 0f && onScreen.x < 1f && onScreen.y > 0f && onScreen.y < 1f, Is.True,
                $"ball at {onScreen}");

            swipe.Push(TrackInput.Jump);
            var gameOver = Object.FindFirstObjectByType<GameOverView>();
            var until = Time.realtimeSinceStartup + 30f;
            while (!gameOver.IsShown && Time.realtimeSinceStartup < until) await UniTask.Yield();
            Assert.That(gameOver.IsShown, Is.True, "the ball did not crash within 30 s");
        });
        // /cmpose:track3D

        private static Button GameButton(MenuView menu, string labelKey)
        {
            foreach (var button in menu.GameButtons)
            {
                if (button.GetComponentInChildren<LocalizedText>(true).Key == labelKey) return button;
            }
            Assert.Fail($"no Play button '{labelKey}' in the menu");
            return null;
        }

        private static async UniTask<T> WaitFor<T>(float seconds) where T : Object
        {
            var until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until)
            {
                var found = Object.FindFirstObjectByType<T>();
                if (found != null) return found;
                await UniTask.Yield();
            }
            return null;
        }
    }
}
