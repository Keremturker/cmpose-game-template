using System;
using System.Globalization;

namespace Kturker.UnityBase.Core.Update
{
    public static class VersionName
    {
        // Part by part as numbers ("1.10" is newer than "1.9"); a missing part counts as 0 ("1.4" equals "1.4.0"), and
        // everything from the first character that is not a digit or '.' is ignored ("2.0.0-beta" equals "2.0.0").
        // Negative: a is older; 0: the same; positive: a is newer.
        public static int Compare(string a, string b)
        {
            var left = Parts(a);
            var right = Parts(b);
            for (var i = 0; i < Math.Max(left.Length, right.Length); i++)
            {
                var difference = (i < left.Length ? left[i] : 0L).CompareTo(i < right.Length ? right[i] : 0L);
                if (difference != 0) return difference;
            }
            return 0;
        }

        private static long[] Parts(string version)
        {
            var text = (version ?? string.Empty).Trim();
            var end = 0;
            while (end < text.Length && ((text[end] >= '0' && text[end] <= '9') || text[end] == '.')) end++;
            var pieces = text.Substring(0, end).Split('.');
            var parts = new long[pieces.Length];
            for (var i = 0; i < pieces.Length; i++)
                parts[i] = long.TryParse(pieces[i], NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0L;
            return parts;
        }
    }
}
