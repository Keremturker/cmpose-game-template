package dev.cmpose.play;

import android.app.Activity;
import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.net.Uri;
import com.google.android.play.core.review.ReviewInfo;
import com.google.android.play.core.review.ReviewManager;
import com.google.android.play.core.review.ReviewManagerFactory;

// Google Play In-App Review for the game's C# code (PlayReviewRequester). Play decides whether the dialog shows (only
// for an app installed from Google Play, a few times a year); the callback says only whether the flow ran.
public final class CmposeReview {
    private CmposeReview() {
    }

    public static void requestReview(final Activity activity, final CmposeCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                final ReviewManager manager = ReviewManagerFactory.create(activity);
                manager.requestReviewFlow().addOnCompleteListener(request -> {
                    if (!request.isSuccessful()) {
                        callback.onResult(false, describe(request.getException()));
                        return;
                    }
                    ReviewInfo info = request.getResult();
                    manager.launchReviewFlow(activity, info).addOnCompleteListener(flow ->
                            callback.onResult(flow.isSuccessful(), flow.isSuccessful() ? "" : describe(flow.getException())));
                });
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    // The Play Store app first, then the web page (an empty fallbackUrl means none); false when no app opens either
    public static boolean openUrl(Activity activity, String url, String fallbackUrl) {
        if (open(activity, url)) return true;
        return fallbackUrl != null && !fallbackUrl.isEmpty() && open(activity, fallbackUrl);
    }

    static String describe(Exception e) {
        return e == null ? "unknown error" : e.getClass().getSimpleName() + ": " + e.getMessage();
    }

    private static boolean open(Activity activity, String url) {
        try {
            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            activity.startActivity(intent);
            return true;
        } catch (ActivityNotFoundException e) {
            return false;
        }
    }
}
