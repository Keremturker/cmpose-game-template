using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Network;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Features.Sample.Network
{
    // The network flag's example: loads one GitHub user when the Game scene starts, Retry after an error.
    // Leaving the scene cancels the call; a late answer changes nothing.
    public sealed class GitHubUserPresenter : Presenter<IGitHubUserCardView>
    {
        public const string User = "octocat";

        private readonly IApiClient api;
        private readonly IRemoteImageLoader images;
        private readonly ILog log;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private bool loading;

        public GitHubUserPresenter(IGitHubUserCardView view, IApiClient api, IRemoteImageLoader images, ILog log) : base(view)
        {
            this.api = api;
            this.images = images;
            this.log = log;
        }

        public override void Start()
        {
            View.RetryClicked += OnRetry;
            LoadAsync().Forget();
        }

        public override void Dispose()
        {
            View.RetryClicked -= OnRetry;
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private void OnRetry()
        {
            if (!loading) LoadAsync().Forget();
        }

        private async UniTaskVoid LoadAsync()
        {
            loading = true;
            View.ShowLoading();
            var token = cancellation.Token;
            var result = await api.GetJson<GitHubUser>($"users/{User}", token);
            if (token.IsCancellationRequested) return;
            loading = false;
            if (!result.IsSuccess)
            {
                if (result.Error.Kind == NetworkErrorKind.Cancelled) return;
                View.ShowError(NetworkKeys.Error(result.Error.Kind), result.Error.StatusCode);
                log.Marker($"CMPOSE_NET user={User} status=error:{result.Error.Kind}");
                return;
            }
            var user = result.Value;
            View.ShowUser(string.IsNullOrEmpty(user.Name) ? user.Login : user.Name, user.Login);
            log.Marker($"CMPOSE_NET user={User} status=ok");
            var avatar = await images.LoadAsync(user.AvatarUrl, token);
            if (avatar != null && !token.IsCancellationRequested) View.ShowAvatar(avatar);
        }
    }
}
