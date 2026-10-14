using System;
using Kturker.UnityBase.Core.Domain;
using Kturker.UnityBase.Core.Preferences;

namespace Kturker.UnityBase.Core.Review
{
    // Call RecordPositiveEvent right after a moment the player enjoyed (a level won, a goal reached), never at launch
    // or in the middle of play. The third positive event asks for the store's review dialog, then every tenth. Apple
    // and Google allow only their own dialog: no "Do you like the game?" screen of your own.
    public sealed class ReviewPrompter
    {
        public const string PositiveEventsKey = "review.positiveEvents";
        public const string NextThresholdKey = "review.nextThreshold";
        public const int FirstThreshold = 3;
        public const int RepeatInterval = 10;

        private readonly IPreferences preferences;
        private readonly IReviewRequester requester;
        private readonly StoreIdentity store;
        private readonly ILog log;

        public ReviewPrompter(IPreferences preferences, IReviewRequester requester, StoreIdentity store, ILog log)
        {
            this.preferences = preferences;
            this.requester = requester;
            this.store = store;
            this.log = log;
        }

        // Settings hides "Rate the app" when false (iOS without an App Store ID)
        public bool CanOpenStorePage => store.HasPage;

        // True when this event asked for the dialog. Stored values out of range read as the defaults (a negative count
        // as 0, a threshold below 3 as 3); a count that reached int.MaxValue stops counting and asking.
        public bool RecordPositiveEvent()
        {
            var stored = preferences.GetInt(PositiveEventsKey, 0);
            if (stored == int.MaxValue) return false;
            var count = Math.Max(stored, 0) + 1;
            preferences.SetInt(PositiveEventsKey, count);
            var threshold = Math.Max(preferences.GetInt(NextThresholdKey, FirstThreshold), FirstThreshold);
            var due = count >= threshold;
            if (due)
            {
                preferences.SetInt(NextThresholdKey, count > int.MaxValue - RepeatInterval ? int.MaxValue : count + RepeatInterval);
                log.Marker("CMPOSE_REVIEW requested");
                requester.RequestReview();
            }
            preferences.Save();
            return due;
        }

        // Settings' "Rate the app"
        public void OpenStorePage()
        {
            if (!store.HasPage)
            {
                log.Warning("No store page: set appStoreId in AppConfig for iOS");
                return;
            }
            requester.OpenStorePage(store.PageUrl, store.FallbackUrl);
        }
    }
}
