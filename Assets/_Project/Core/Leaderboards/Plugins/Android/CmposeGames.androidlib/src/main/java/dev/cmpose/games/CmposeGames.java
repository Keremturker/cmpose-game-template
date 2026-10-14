package dev.cmpose.games;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.PlayGamesSdk;

// Google Play Games Services v2 for the game's C# code (AndroidGameServicesGateway). Every method runs on the UI
// thread and answers exactly once through the callback: ok=true, or ok=false with a short reason (empty when the
// answer is simply "no", e.g. the player is not signed in). Nothing throws.
public final class CmposeGames {
    // Play reports a screen's result to the activity, which ignores it: Play shows its own screens
    private static final int REQUEST_CODE = 0x434E;

    private CmposeGames() {
    }

    // Starts the SDK, which then signs the player in on its own (silent); called once, only with real IDs
    public static void initialize(final Activity activity, final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGamesSdk.initialize(activity);
                callback.onResult(true, "");
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    // Whether the silent sign-in has signed the player in
    public static void isAuthenticated(final Activity activity, final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getGamesSignInClient(activity).isAuthenticated().addOnCompleteListener(task ->
                        callback.onResult(task.isSuccessful() && task.getResult().isAuthenticated(),
                                task.isSuccessful() ? "" : describe(task.getException())));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    // The player asked: Play may show its sign-in screen
    public static void signIn(final Activity activity, final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getGamesSignInClient(activity).signIn().addOnCompleteListener(task ->
                        callback.onResult(task.isSuccessful() && task.getResult().isAuthenticated(),
                                task.isSuccessful() ? "" : describe(task.getException())));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    public static void submitScore(final Activity activity, final String leaderboardId, final long score,
            final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getLeaderboardsClient(activity).submitScoreImmediate(leaderboardId, score)
                        .addOnCompleteListener(task -> callback.onResult(task.isSuccessful(),
                                task.isSuccessful() ? "" : describe(task.getException())));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    public static void unlock(final Activity activity, final String achievementId, final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getAchievementsClient(activity).unlockImmediate(achievementId)
                        .addOnCompleteListener(task -> callback.onResult(task.isSuccessful(),
                                task.isSuccessful() ? "" : describe(task.getException())));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    public static void increment(final Activity activity, final String achievementId, final int steps,
            final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getAchievementsClient(activity).incrementImmediate(achievementId, steps)
                        .addOnCompleteListener(task -> callback.onResult(task.isSuccessful(),
                                task.isSuccessful() ? "" : describe(task.getException())));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    // The callback answers when the screen has been started, not when the player closes it
    public static void showLeaderboard(final Activity activity, final String leaderboardId, final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getLeaderboardsClient(activity).getLeaderboardIntent(leaderboardId)
                        .addOnSuccessListener(intent -> open(activity, intent, callback))
                        .addOnFailureListener(failure -> callback.onResult(false, describe(failure)));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    public static void showAchievements(final Activity activity, final CmposeGamesCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                PlayGames.getAchievementsClient(activity).getAchievementsIntent()
                        .addOnSuccessListener(intent -> open(activity, intent, callback))
                        .addOnFailureListener(failure -> callback.onResult(false, describe(failure)));
            } catch (RuntimeException e) {
                callback.onResult(false, describe(e));
            }
        });
    }

    private static void open(Activity activity, Intent intent, CmposeGamesCallback callback) {
        try {
            activity.startActivityForResult(intent, REQUEST_CODE);
            callback.onResult(true, "");
        } catch (RuntimeException e) {
            callback.onResult(false, describe(e));
        }
    }

    private static String describe(Exception e) {
        return e == null ? "unknown error" : e.getClass().getSimpleName() + ": " + e.getMessage();
    }
}
