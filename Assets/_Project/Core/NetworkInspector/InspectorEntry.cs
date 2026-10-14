using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kturker.UnityBase.Core.Network;

namespace Kturker.UnityBase.Core.NetworkInspector
{
    // One exchange as the panel shows it. A developer tool: the detail is English and written once, the
    // Authorization header is masked and bodies are cut at 4 KB.
    public sealed class InspectorEntry
    {
        public const int BodyLimit = 4096;
        public const string Masked = "***";

        private InspectorEntry(string summary, string detail)
        {
            Summary = summary;
            Detail = detail;
        }

        public string Summary { get; }
        public string Detail { get; }

        public static InspectorEntry From(HttpRequest request, HttpResponse response)
        {
            var status = response.Failure == TransportFailure.None
                ? response.StatusCode.ToString(CultureInfo.InvariantCulture)
                : response.Failure.ToString();
            var ms = Math.Round(response.Milliseconds).ToString("0", CultureInfo.InvariantCulture);
            var detail = new StringBuilder();
            detail.Append(request.Method).Append(' ').Append(request.Url).Append('\n');
            detail.Append("Status: ").Append(status).Append(" (").Append(ms).Append(" ms)\n\n");
            detail.Append("Request headers\n");
            AppendHeaders(detail, request.Headers);
            detail.Append("\nResponse headers\n");
            AppendHeaders(detail, response.Headers);
            detail.Append("\nBody\n").Append(Cut(response.Body));
            return new InspectorEntry($"{request.Method} {status} {ms}ms {PathOf(request.Url)}", detail.ToString());
        }

        private static string PathOf(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.PathAndQuery : url;

        private static void AppendHeaders(StringBuilder detail, IReadOnlyDictionary<string, string> headers)
        {
            foreach (var header in headers.OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase))
            {
                var masked = string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase);
                detail.Append(header.Key).Append(": ").Append(masked ? Masked : header.Value).Append('\n');
            }
        }

        private static string Cut(string body)
        {
            if (body == null) return string.Empty;
            return body.Length <= BodyLimit ? body : body.Substring(0, BodyLimit) + "\n... (truncated)";
        }
    }
}
