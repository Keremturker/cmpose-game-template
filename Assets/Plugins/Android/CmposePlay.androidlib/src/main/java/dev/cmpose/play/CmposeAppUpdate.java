package dev.cmpose.play;

import android.app.Activity;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.Build;
import com.google.android.play.core.appupdate.AppUpdateInfo;
import com.google.android.play.core.appupdate.AppUpdateManager;
import com.google.android.play.core.appupdate.AppUpdateManagerFactory;
import com.google.android.play.core.appupdate.AppUpdateOptions;
import com.google.android.play.core.install.model.AppUpdateType;
import com.google.android.play.core.install.model.InstallStatus;
import com.google.android.play.core.install.model.UpdateAvailability;

// Google Play In-App Updates for the game's C# code (PlayPlatformUpdater). Play offers an update only to an app
// installed from Google Play (an internal testing track works); elsewhere start() reports the error.
public final class CmposeAppUpdate {
    // Play reports the flow's result to the activity, which ignores it: Play shows its own screens
    private static final int REQUEST_CODE = 0x434D;

    private CmposeAppUpdate() {
    }

    // The installed app's versionCode; -1 when it cannot be read
    public static long versionCode(Activity activity) {
        try {
            PackageInfo info = activity.getPackageManager().getPackageInfo(activity.getPackageName(), 0);
            return Build.VERSION.SDK_INT >= Build.VERSION_CODES.P ? info.getLongVersionCode() : legacyVersionCode(info);
        } catch (PackageManager.NameNotFoundException e) {
            return -1;
        }
    }

    // Starts Play's immediate (true) or flexible (false) update when Play offers it. The callback gets ok=true with
    // "started", or "installing" when a flexible update downloaded earlier is installed now; ok=false with
    // "not-offered", "not-started" or the error.
    public static void start(final Activity activity, final boolean immediate, final CmposeCallback callback) {
        activity.runOnUiThread(() -> {
            try {
                final AppUpdateManager manager = AppUpdateManagerFactory.create(activity);
                manager.getAppUpdateInfo().addOnCompleteListener(task -> {
                    if (!task.isSuccessful()) {
                        callback.onResult(false, CmposeReview.describe(task.getException()));
                        return;
                    }
                    handle(activity, manager, task.getResult(), immediate, callback);
                });
            } catch (RuntimeException e) {
                callback.onResult(false, CmposeReview.describe(e));
            }
        });
    }

    private static void handle(Activity activity, AppUpdateManager manager, AppUpdateInfo info, boolean immediate,
            CmposeCallback callback) {
        if (info.installStatus() == InstallStatus.DOWNLOADED) {
            manager.completeUpdate();
            callback.onResult(true, "installing");
            return;
        }
        int type = immediate ? AppUpdateType.IMMEDIATE : AppUpdateType.FLEXIBLE;
        int availability = info.updateAvailability();
        boolean offered = availability == UpdateAvailability.UPDATE_AVAILABLE
                || (immediate && availability == UpdateAvailability.DEVELOPER_TRIGGERED_UPDATE_IN_PROGRESS);
        if (!offered || !info.isUpdateTypeAllowed(type)) {
            callback.onResult(false, "not-offered");
            return;
        }
        try {
            boolean started = manager.startUpdateFlowForResult(info, activity, AppUpdateOptions.defaultOptions(type), REQUEST_CODE);
            callback.onResult(started, started ? "started" : "not-started");
        } catch (Exception e) {
            callback.onResult(false, CmposeReview.describe(e));
        }
    }

    @SuppressWarnings("deprecation")
    private static long legacyVersionCode(PackageInfo info) {
        return info.versionCode;
    }
}
