using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Network;

namespace Kturker.UnityBase.Core.NetworkInspector
{
    // The last 50 exchanges of IApiClient, newest first
    public sealed class InspectorObserver : IRequestObserver
    {
        public const int Capacity = 50;
        private readonly List<InspectorEntry> entries = new List<InspectorEntry>();

        public event Action Changed;

        public IReadOnlyList<InspectorEntry> Entries => entries;

        public void OnCompleted(HttpRequest request, HttpResponse response)
        {
            entries.Insert(0, InspectorEntry.From(request, response));
            if (entries.Count > Capacity) entries.RemoveAt(entries.Count - 1);
            Changed?.Invoke();
        }

        public void Clear()
        {
            if (entries.Count == 0) return;
            entries.Clear();
            Changed?.Invoke();
        }
    }
}
