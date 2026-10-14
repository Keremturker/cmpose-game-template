using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Review
{
    // The editor and platforms without a store dialog: logs what a device would do
    public sealed class LogReviewRequester : IReviewRequester
    {
        private readonly ILog log;

        public LogReviewRequester(ILog log) => this.log = log;

        public void RequestReview() => log.Info("Review dialog: shown by the store on a device");

        public void OpenStorePage(string url, string fallbackUrl) => log.Info($"Store page: {url}");
    }
}
