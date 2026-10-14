using System;
using System.Linq;

namespace Kturker.UnityBase.Core.NetworkInspector
{
    // The newest RowCount entries as rows; the chosen one's detail below (the newest until a row is chosen)
    public sealed class InspectorPresenter : IDisposable
    {
        private readonly IInspectorView view;
        private readonly InspectorObserver observer;
        private InspectorEntry chosen;
        private bool picked;

        public InspectorPresenter(IInspectorView view, InspectorObserver observer)
        {
            this.view = view;
            this.observer = observer;
        }

        public void Start()
        {
            view.OpenClicked += Open;
            view.CloseClicked += Close;
            view.ClearClicked += observer.Clear;
            view.RowClicked += Choose;
            observer.Changed += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            view.OpenClicked -= Open;
            view.CloseClicked -= Close;
            view.ClearClicked -= observer.Clear;
            view.RowClicked -= Choose;
            observer.Changed -= Refresh;
        }

        private void Open() => view.SetOpen(true);

        private void Close() => view.SetOpen(false);

        private void Refresh()
        {
            var entries = observer.Entries;
            var shown = entries.Take(view.RowCount).ToList();
            view.ShowRows(shown.Select(e => e.Summary).ToList());
            if (shown.Count == 0)
            {
                chosen = null;
                picked = false;
                view.ShowEmpty();
                return;
            }
            if (!picked || !entries.Contains(chosen))
            {
                chosen = shown[0];
                picked = false;
            }
            view.ShowDetail(chosen.Detail);
        }

        private void Choose(int index)
        {
            var entries = observer.Entries;
            if (index < 0 || index >= Math.Min(entries.Count, view.RowCount)) return;
            chosen = entries[index];
            picked = true;
            view.ShowDetail(chosen.Detail);
        }
    }
}
