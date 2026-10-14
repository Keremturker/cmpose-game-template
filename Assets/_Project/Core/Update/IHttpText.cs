using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Update
{
    // A GET that returns the body of a 200 answer, or null on any failure (offline, timeout, another status)
    public interface IHttpText
    {
        UniTask<string> GetAsync(string url, CancellationToken cancellation);
    }
}
