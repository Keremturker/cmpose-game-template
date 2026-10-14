using System;

namespace Kturker.UnityBase.Core.Network
{
    // The API's base URLs. The generator writes both lines from the wizard's one field (like cmpose's ApiConfig.kt);
    // point ReleaseBaseUrl at production later. Debug.isDebugBuild picks one (NetworkInstaller).
    public static class ApiConfig
    {
        public const string DebugBaseUrl = "https://api.github.com/";
        public const string ReleaseBaseUrl = "https://api.github.com/";

        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        public static string BaseUrl(bool debugBuild) => debugBuild ? DebugBaseUrl : ReleaseBaseUrl;
    }
}
