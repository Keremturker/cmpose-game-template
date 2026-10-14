namespace Kturker.UnityBase.Core.Review
{
    // The platform's own review dialog and store page. The store decides whether the dialog shows, and the game never
    // learns whether the player rated it.
    public interface IReviewRequester
    {
        void RequestReview();

        // Opens url, or fallbackUrl (may be null) when no app can open url
        void OpenStorePage(string url, string fallbackUrl);
    }
}
