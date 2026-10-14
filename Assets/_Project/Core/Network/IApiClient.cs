using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Network
{
    public interface IApiClient
    {
        // GET <base URL>/<path>, JSON body into T. Never throws for network or data problems.
        UniTask<ApiResult<T>> GetJson<T>(string path, CancellationToken cancellation) where T : class;
    }
}
