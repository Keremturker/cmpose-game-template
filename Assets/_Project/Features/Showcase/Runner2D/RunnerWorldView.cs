using System.Collections.Generic;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Showcase.Runner2D.Domain;
using UnityEngine;
using VContainer;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    // Draws the simulation with generated sprites, one world unit per unit: ground, runner, obstacles and stars. The
    // camera (GameRig's) is made orthographic with size 5, so the upstream 3D rig shows the same picture as the 2D one.
    // Colours follow the theme; premium paints the runner gold.
    public sealed class RunnerWorldView : MonoBehaviour, IRunnerWorldView
    {
        private const float CameraSize = 5f;
        private const float GroundScreenY = -2.5f;
        private const float RunnerFromLeft = 1.5f;
        private static readonly Color Gold = new Color(1f, 0.76f, 0.2f);
        private static readonly Color StarColor = new Color(1f, 0.85f, 0.35f);

        // URP's unlit sprite material (SceneBuilder wires it), so sprites need no 2D light in the upstream 3D renderer
        [SerializeField] private Material spriteMaterial;

        private readonly List<SpriteRenderer> obstacles = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> stars = new List<SpriteRenderer>();
        private IThemeSource theme;
        private Sprite square;
        private Sprite circle;
        private SpriteRenderer ground;
        private SpriteRenderer runner;
        private float runnerX;
        private bool premium;

        // The scope injects after or before Awake depending on object order, so both paint when they can
        [Inject]
        public void Construct(IThemeSource themeSource)
        {
            theme = themeSource;
            theme.Changed += Paint;
            Paint();
        }

        private void Awake()
        {
            square = ShapeSprites.Square();
            circle = ShapeSprites.Circle();
            var halfWidth = CameraSize;
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = CameraSize;
                halfWidth = CameraSize * camera.aspect;
            }
            runnerX = -halfWidth + RunnerFromLeft;
            ground = Create("Ground", square, 0);
            ground.transform.position = new Vector3(0f, GroundScreenY - 0.5f, 0f);
            ground.transform.localScale = new Vector3(halfWidth * 2f + 2f, 1f, 1f);
            runner = Create("Runner", square, 2);
            Paint();
        }

        private void OnDestroy()
        {
            if (theme != null) theme.Changed -= Paint;
            ShapeSprites.Destroy(square);
            ShapeSprites.Destroy(circle);
        }

        public void SetPremium(bool value)
        {
            premium = value;
            Paint();
        }

        public void Render(RunnerSimulation sim)
        {
            var size = sim.Config.RunnerSize;
            runner.transform.position = new Vector3(runnerX, GroundScreenY + sim.RunnerY + size / 2f, 0f);
            runner.transform.localScale = new Vector3(size, size, 1f);
            // Blinks while a continue protects it
            runner.enabled = !sim.Invulnerable || Mathf.Repeat(Time.unscaledTime * 8f, 1f) > 0.5f;
            Fit(obstacles, sim.Obstacles.Count, square, "Obstacle");
            for (var i = 0; i < sim.Obstacles.Count; i++)
            {
                var o = sim.Obstacles[i];
                var t = obstacles[i].transform;
                t.position = new Vector3(runnerX + o.X - sim.Distance + o.Width / 2f, GroundScreenY + o.Bottom + o.Height / 2f, 0f);
                t.localScale = new Vector3(o.Width, o.Height, 1f);
            }
            Fit(stars, sim.Stars.Count, circle, "Star");
            var starSize = sim.Config.StarSize;
            for (var i = 0; i < sim.Stars.Count; i++)
            {
                var s = sim.Stars[i];
                var t = stars[i].transform;
                t.position = new Vector3(runnerX + s.X - sim.Distance, GroundScreenY + s.Y, 0f);
                t.localScale = new Vector3(starSize, starSize, 1f);
            }
        }

        // Keeps exactly count renderers of the pool active, creating more when needed
        private void Fit(List<SpriteRenderer> pool, int count, Sprite sprite, string name)
        {
            var grew = false;
            while (pool.Count < count)
            {
                pool.Add(Create(name, sprite, 1));
                grew = true;
            }
            if (grew) Paint();
            for (var i = 0; i < pool.Count; i++) pool[i].gameObject.SetActive(i < count);
        }

        private SpriteRenderer Create(string objectName, Sprite sprite, int order)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = order;
            if (spriteMaterial != null) spriteRenderer.sharedMaterial = spriteMaterial;
            return spriteRenderer;
        }

        private void Paint()
        {
            if (theme == null || runner == null) return;
            var palette = theme.Current;
            ground.color = palette.Get(ThemeRole.Track);
            runner.color = premium ? Gold : palette.Get(ThemeRole.Primary);
            foreach (var o in obstacles) o.color = palette.Get(ThemeRole.Text);
            foreach (var s in stars) s.color = StarColor;
        }
    }
}
