using System;
using System.Collections.Generic;

namespace Kturker.UnityBase.Core.DeepLink
{
    // A parsed scheme://path?query link. The path is lower case without leading or trailing '/'. The scheme is not
    // checked: the OS only delivers the scheme the app registered. Not called DeepLink because the namespace is.
    public sealed class DeepLinkRequest
    {
        private DeepLinkRequest(string url, string path, IReadOnlyDictionary<string, string> query)
        {
            Url = url;
            Path = path;
            Query = query;
        }

        public string Url { get; }
        public string Path { get; }
        public IReadOnlyDictionary<string, string> Query { get; }

        public static bool TryParse(string url, out DeepLinkRequest request)
        {
            request = null;
            if (string.IsNullOrWhiteSpace(url)) return false;
            var text = url.Trim();
            var separator = text.IndexOf("://", StringComparison.Ordinal);
            if (separator <= 0) return false;
            var rest = text.Substring(separator + 3);
            var hash = rest.IndexOf('#');
            if (hash >= 0) rest = rest.Substring(0, hash);
            var question = rest.IndexOf('?');
            var path = (question >= 0 ? rest.Substring(0, question) : rest).Trim('/').ToLowerInvariant();
            if (path.Length == 0) return false;
            var query = new Dictionary<string, string>(StringComparer.Ordinal);
            if (question >= 0)
            {
                foreach (var pair in rest.Substring(question + 1).Split('&'))
                {
                    if (pair.Length == 0) continue;
                    var equals = pair.IndexOf('=');
                    var key = Uri.UnescapeDataString(equals >= 0 ? pair.Substring(0, equals) : pair);
                    query[key] = equals >= 0 ? Uri.UnescapeDataString(pair.Substring(equals + 1)) : string.Empty;
                }
            }
            request = new DeepLinkRequest(text, path, query);
            return true;
        }
    }
}
