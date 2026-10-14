package dev.cmpose.games;

// Implemented in C# with AndroidJavaProxy; called on the Android UI thread, exactly once per request
public interface CmposeGamesCallback {
    void onResult(boolean ok, String detail);
}
