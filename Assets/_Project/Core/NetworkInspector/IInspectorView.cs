using System;
using System.Collections.Generic;

namespace Kturker.UnityBase.Core.NetworkInspector
{
    public interface IInspectorView
    {
        event Action OpenClicked;
        event Action CloseClicked;
        event Action ClearClicked;
        event Action<int> RowClicked;
        int RowCount { get; }
        void SetOpen(bool open);
        void ShowRows(IReadOnlyList<string> summaries);
        void ShowDetail(string text);
        void ShowEmpty();
    }
}
