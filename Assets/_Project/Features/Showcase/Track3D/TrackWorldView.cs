using System.Collections.Generic;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Features.Showcase.Track3D.Domain;
using UnityEngine;
using VContainer;

namespace Kturker.UnityBase.Features.Showcase.Track3D
{
    // Draws the track with Unity's built-in cube and sphere meshes and one URP Lit material (SceneBuilder wires all
    // three): ground pieces cut at the gaps' edges, cross stripes, blocks, gems and the rolling ball, all pooled, so a
    // frame allocates nothing once the pools have grown. The ball stays at the origin and the track moves past it;
    // GameRig's camera follows from behind and above. Colours come from the theme through one MaterialPropertyBlock;
    // premium paints the ball gold.
    public sealed class TrackWorldView : MonoBehaviour, ITrackWorldView
    {
        private const float Behind = 10f;
        private const float GroundDepth = 0.4f;
        private const float StripeSpacing = 4f;
        private const float StripeLength = 0.25f;
        private const float BlockShare = 0.85f;
        private const float GemLift = 0.15f;
        private const float GemSpin = 120f;
        private const float CameraHeight = 3.5f;
        private const float CameraBack = 6f;
        private const float CameraPitch = 18f;
        // Share of the ball's sideways move the camera follows, so the three lanes stay in view
        private const float CameraFollow = 0.6f;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly Color Gold = new Color(1f, 0.76f, 0.2f);
        private static readonly Color GemColor = new Color(0.3f, 0.85f, 0.95f);
        // A band round the ball, so its roll shows
        private static readonly Vector3 BandScale = new Vector3(1.04f, 1.04f, 0.3f);

        [SerializeField] private Material material;
        [SerializeField] private Mesh cube;
        [SerializeField] private Mesh sphere;

        private readonly List<MeshRenderer> grounds = new List<MeshRenderer>();
        private readonly List<MeshRenderer> stripes = new List<MeshRenderer>();
        private readonly List<MeshRenderer> blocks = new List<MeshRenderer>();
        private readonly List<MeshRenderer> gems = new List<MeshRenderer>();
        private MaterialPropertyBlock colours;
        private IThemeSource theme;
        private Camera view;
        private MeshRenderer ball;
        private MeshRenderer band;
        private bool premium;
        private bool grew;

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
            colours = new MaterialPropertyBlock();
            view = Camera.main;
            ball = Create("Ball", sphere, transform);
            band = Create("Band", sphere, ball.transform);
            band.transform.localScale = BandScale;
            Paint();
        }

        private void OnDestroy()
        {
            if (theme != null) theme.Changed -= Paint;
        }

        public void SetPremium(bool value)
        {
            premium = value;
            Paint();
        }

        public void Render(TrackSimulation sim)
        {
            var config = sim.Config;
            var from = sim.Distance - Behind;
            var to = sim.Distance + config.SpawnAhead;
            var width = config.LaneWidth * TrackSimulation.Lanes;
            PlaceBall(sim);

            var pieces = 0;
            var cursor = from;
            for (var i = 0; i < sim.Rows.Count; i++)
            {
                var row = sim.Rows[i];
                if (row.Kind != RowKind.Gap || row.End <= cursor || row.Start >= to) continue;
                if (row.Start > cursor) Ground(pieces++, cursor, row.Start, width, sim.Distance);
                cursor = row.End;
            }
            if (to > cursor) Ground(pieces++, cursor, to, width, sim.Distance);
            Hide(grounds, pieces);

            var count = 0;
            for (var z = Mathf.Ceil(from / StripeSpacing) * StripeSpacing; z < to; z += StripeSpacing)
            {
                if (InGap(sim, z)) continue;
                var stripe = Take(stripes, count++, cube, "Stripe").transform;
                stripe.localPosition = new Vector3(0f, 0.01f, z - sim.Distance);
                stripe.localScale = new Vector3(width, 0.02f, StripeLength);
            }
            Hide(stripes, count);

            count = 0;
            for (var i = 0; i < sim.Rows.Count; i++)
            {
                var row = sim.Rows[i];
                if (row.Kind != RowKind.Blocks || row.End < from || row.Start > to) continue;
                for (var lane = 0; lane < TrackSimulation.Lanes; lane++)
                {
                    if (!row.Covers(lane)) continue;
                    var block = Take(blocks, count++, cube, "Block").transform;
                    block.localPosition = new Vector3(config.LaneX(lane), config.BlockHeight / 2f, row.Start + row.Length / 2f - sim.Distance);
                    block.localScale = new Vector3(config.LaneWidth * BlockShare, config.BlockHeight, row.Length);
                }
            }
            Hide(blocks, count);

            var spin = Quaternion.Euler(0f, Time.time * GemSpin, 45f);
            var size = config.GemSize * 0.7f;
            for (var i = 0; i < sim.Gems.Count; i++)
            {
                var gem = sim.Gems[i];
                var t = Take(gems, i, cube, "Gem").transform;
                t.localPosition = new Vector3(config.LaneX(gem.Lane), config.GemSize / 2f + GemLift, gem.Z - sim.Distance);
                t.localRotation = spin;
                t.localScale = new Vector3(size, size, size);
            }
            Hide(gems, sim.Gems.Count);

            if (grew)
            {
                grew = false;
                Paint();
            }
            Follow(sim);
        }

        private void PlaceBall(TrackSimulation sim)
        {
            var size = sim.Config.BallSize;
            var t = ball.transform;
            t.localPosition = new Vector3(sim.X, sim.Y + size / 2f, 0f);
            t.localScale = new Vector3(size, size, size);
            // Rolls one radian per radius travelled
            t.localRotation = Quaternion.Euler(Mathf.Repeat(sim.Distance / (size / 2f) * Mathf.Rad2Deg, 360f), 0f, 0f);
            // Blinks while a continue protects it
            var shown = !sim.Invulnerable || Mathf.Repeat(Time.unscaledTime * 8f, 1f) > 0.5f;
            ball.enabled = shown;
            band.enabled = shown;
        }

        private void Follow(TrackSimulation sim)
        {
            // The camera can be destroyed or replaced after Start; look it up again then
            if (view == null) view = Camera.main;
            if (view == null) return;
            view.transform.SetPositionAndRotation(new Vector3(sim.X * CameraFollow, CameraHeight, -CameraBack),
                Quaternion.Euler(CameraPitch, 0f, 0f));
        }

        private void Ground(int index, float start, float end, float width, float distance)
        {
            var t = Take(grounds, index, cube, "Ground").transform;
            t.localPosition = new Vector3(0f, -GroundDepth / 2f, (start + end) / 2f - distance);
            t.localScale = new Vector3(width, GroundDepth, end - start);
        }

        private static bool InGap(TrackSimulation sim, float z)
        {
            for (var i = 0; i < sim.Rows.Count; i++)
            {
                var row = sim.Rows[i];
                if (row.Kind == RowKind.Gap && z + StripeLength / 2f > row.Start && z - StripeLength / 2f < row.End) return true;
            }
            return false;
        }

        // The pool's index-th renderer, created when missing, shown
        private MeshRenderer Take(List<MeshRenderer> pool, int index, Mesh mesh, string objectName)
        {
            while (pool.Count <= index)
            {
                pool.Add(Create(objectName, mesh, transform));
                grew = true;
            }
            var meshRenderer = pool[index];
            if (!meshRenderer.gameObject.activeSelf) meshRenderer.gameObject.SetActive(true);
            return meshRenderer;
        }

        private static void Hide(List<MeshRenderer> pool, int used)
        {
            for (var i = used; i < pool.Count; i++)
            {
                if (pool[i].gameObject.activeSelf) pool[i].gameObject.SetActive(false);
            }
        }

        private MeshRenderer Create(string objectName, Mesh mesh, Transform parent)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            return meshRenderer;
        }

        private void Paint()
        {
            if (theme == null || ball == null) return;
            var palette = theme.Current;
            Tint(ball, premium ? Gold : palette.Get(ThemeRole.Primary));
            Tint(band, palette.Get(ThemeRole.OnPrimary));
            TintAll(grounds, palette.Get(ThemeRole.Track));
            TintAll(stripes, palette.Get(ThemeRole.TextMuted));
            TintAll(blocks, palette.Get(ThemeRole.Text));
            TintAll(gems, GemColor);
        }

        private void TintAll(List<MeshRenderer> pool, Color color)
        {
            foreach (var meshRenderer in pool) Tint(meshRenderer, color);
        }

        private void Tint(MeshRenderer target, Color color)
        {
            colours.SetColor(BaseColor, color);
            target.SetPropertyBlock(colours);
        }
    }
}
