using System;
using System.Collections.Generic;
using System.Linq;
using Kturker.UnityBase.Core.Network;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.NetworkInspector.Tests
{
    public sealed class InspectorPresenterTests
    {
        private sealed class FakeInspectorView : IInspectorView
        {
            public bool Open;
            public List<string> Rows = new List<string>();
            public string Detail;
            public bool Empty;
            public event Action OpenClicked;
            public event Action CloseClicked;
            public event Action ClearClicked;
            public event Action<int> RowClicked;
            public int RowCount => 3;
            public void SetOpen(bool open) => Open = open;
            public void ShowRows(IReadOnlyList<string> summaries) => Rows = summaries.ToList();

            public void ShowDetail(string text)
            {
                Detail = text;
                Empty = false;
            }

            public void ShowEmpty()
            {
                Detail = null;
                Empty = true;
            }

            public void ClickOpen() => OpenClicked?.Invoke();
            public void ClickClose() => CloseClicked?.Invoke();
            public void ClickClear() => ClearClicked?.Invoke();
            public void ClickRow(int index) => RowClicked?.Invoke(index);
        }

        private static void Request(InspectorObserver observer, string path) => observer.OnCompleted(
            new HttpRequest("GET", "https://api.example.test/" + path, null),
            new HttpResponse(TransportFailure.None, 200, path, null, 10));

        private static InspectorPresenter Start(out FakeInspectorView view, out InspectorObserver observer)
        {
            view = new FakeInspectorView();
            observer = new InspectorObserver();
            var presenter = new InspectorPresenter(view, observer);
            presenter.Start();
            return presenter;
        }

        [Test]
        public void Without_requests_the_list_is_empty()
        {
            Start(out var view, out _);

            Assert.That(view.Rows, Is.Empty);
            Assert.That(view.Empty, Is.True);
        }

        [Test]
        public void New_requests_show_newest_first_up_to_the_rows_and_select_the_newest()
        {
            Start(out var view, out var observer);
            foreach (var path in new[] { "alpha", "beta", "gamma", "delta" }) Request(observer, path);

            Assert.That(view.Rows.Count, Is.EqualTo(3));
            Assert.That(view.Rows[0], Does.EndWith("/delta"));
            Assert.That(view.Detail, Is.EqualTo(observer.Entries[0].Detail));
        }

        [Test]
        public void A_chosen_row_stays_chosen_when_a_new_request_arrives()
        {
            Start(out var view, out var observer);
            Request(observer, "alpha");
            Request(observer, "beta");
            view.ClickRow(1);
            Request(observer, "gamma");

            Assert.That(view.Detail, Does.Contain("/alpha"));
            view.ClickRow(7);
            Assert.That(view.Detail, Does.Contain("/alpha"));
        }

        [Test]
        public void Clear_empties_the_list()
        {
            Start(out var view, out var observer);
            Request(observer, "a");
            view.ClickClear();

            Assert.That(view.Rows, Is.Empty);
            Assert.That(view.Empty, Is.True);
        }

        [Test]
        public void Open_and_close_show_and_hide_the_panel()
        {
            Start(out var view, out _);
            view.ClickOpen();
            Assert.That(view.Open, Is.True);
            view.ClickClose();
            Assert.That(view.Open, Is.False);
        }

        [Test]
        public void After_dispose_new_requests_do_not_reach_the_view()
        {
            var presenter = Start(out var view, out var observer);
            presenter.Dispose();
            Request(observer, "a");

            Assert.That(view.Rows, Is.Empty);
        }
    }
}
