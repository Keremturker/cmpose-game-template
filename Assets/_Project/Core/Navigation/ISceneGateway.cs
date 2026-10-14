using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kturker.UnityBase.Core.Navigation
{
    // The engine's scene manager behind an interface, so SceneLoader runs in EditMode tests
    public interface ISceneGateway
    {
        UniTask LoadAsync(string sceneName, CancellationToken cancellation);
    }
}
