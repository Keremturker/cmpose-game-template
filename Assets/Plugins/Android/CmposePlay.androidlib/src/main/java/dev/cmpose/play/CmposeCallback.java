package dev.cmpose.play;

// Implemented in C# with AndroidJavaProxy; called on the Android UI thread
public interface CmposeCallback {
    void onResult(boolean ok, String detail);
}
