# Google Mobile Ads and UMP: the plugin's Unity bridge calls these through JNI, so R8 must keep them (the plugin's
# AAR carries the same rules; these keep the flag safe if an update drops them)
-keep class com.google.unity.ads.** { *; }
-keep class com.google.unity.ump.** { *; }
-keep public class com.google.android.gms.ads.** { public *; }
-keep public class com.google.android.ump.** { public *; }
# The plugin's AAR also carries its bridge to the Next-Gen SDK (com.google.unity.ads.nextgen), which this flag does not
# ship (it uses the standard SDK): R8 must not fail on those missing classes
-dontwarn com.google.android.libraries.ads.mobile.sdk.**
