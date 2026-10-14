using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Navigation
{
    // One startup task run by BootEntry before the menu opens (consent, purchases, ...)
    public interface IBootStep
    {
        string Name { get; }
        UniTask RunAsync(CancellationToken cancellation);
    }
}
