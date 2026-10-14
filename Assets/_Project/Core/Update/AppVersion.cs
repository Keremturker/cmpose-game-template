namespace Kturker.UnityBase.Core.Update
{
    // Name: the version players see (CFBundleShortVersionString on iOS). Code: Android's versionCode, null when unknown.
    public sealed class AppVersion
    {
        public AppVersion(string name, long? code)
        {
            Name = name ?? string.Empty;
            Code = code;
        }

        public string Name { get; }
        public long? Code { get; }
    }
}
