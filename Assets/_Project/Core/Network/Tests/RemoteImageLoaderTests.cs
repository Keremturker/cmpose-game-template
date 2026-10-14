using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;

namespace Kturker.UnityBase.Core.Network.Tests
{
    public sealed class RemoteImageLoaderTests
    {
        private readonly List<Texture2D> made = new List<Texture2D>();

        private sealed class FakeTextureSource : ITextureSource
        {
            public readonly List<string> Downloads = new List<string>();
            public Func<string, Texture2D> Make;

            public UniTask<Texture2D> DownloadAsync(string url, CancellationToken cancellation)
            {
                Downloads.Add(url);
                return UniTask.FromResult(Make(url));
            }
        }

        [TearDown]
        public void DestroyTextures()
        {
            foreach (var texture in made)
            {
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            }
            made.Clear();
        }

        private RemoteImageLoader Make(int capacity, out FakeTextureSource source, out List<Texture2D> released, out FakeLog log)
        {
            source = new FakeTextureSource
            {
                Make = url =>
                {
                    if (url.Contains("broken")) return null;
                    var texture = new Texture2D(1, 1) { name = url };
                    made.Add(texture);
                    return texture;
                }
            };
            var gone = new List<Texture2D>();
            released = gone;
            log = new FakeLog();
            return new RemoteImageLoader(source, log, texture => gone.Add(texture), capacity);
        }

        private static Texture2D Load(RemoteImageLoader loader, string url) => loader.LoadAsync(url, default).GetAwaiter().GetResult();

        [Test]
        public void A_loaded_image_is_reused_without_a_second_download()
        {
            var loader = Make(2, out var source, out var released, out _);
            var first = Load(loader, "https://img.test/a");

            Assert.That(first, Is.Not.Null);
            Assert.That(Load(loader, "https://img.test/a"), Is.SameAs(first));
            Assert.That(source.Downloads, Is.EqualTo(new[] { "https://img.test/a" }));
            Assert.That(released, Is.Empty);
        }

        [Test]
        public void The_least_recently_used_image_is_released_beyond_the_capacity()
        {
            var loader = Make(2, out var source, out var released, out _);
            Load(loader, "https://img.test/a");
            var b = Load(loader, "https://img.test/b");
            Load(loader, "https://img.test/a");
            Load(loader, "https://img.test/c");

            Assert.That(released, Is.EqualTo(new[] { b }));
            Load(loader, "https://img.test/b");
            Assert.That(source.Downloads.Count, Is.EqualTo(4));
        }

        [Test]
        public void A_failed_download_returns_null_logs_and_is_tried_again_next_time()
        {
            var loader = Make(2, out var source, out _, out var log);

            Assert.That(Load(loader, "https://img.test/broken"), Is.Null);
            Assert.That(log.Lines, Has.Some.StartsWith("WARNING"));
            Assert.That(log.Lines, Has.None.Contains("img.test"));
            Load(loader, "https://img.test/broken");
            Assert.That(source.Downloads.Count, Is.EqualTo(2));
        }

        [TestCase(null)]
        [TestCase("")]
        public void An_empty_url_returns_null_without_a_download(string url)
        {
            var loader = Make(2, out var source, out _, out _);

            Assert.That(Load(loader, url), Is.Null);
            Assert.That(source.Downloads, Is.Empty);
        }
    }
}
