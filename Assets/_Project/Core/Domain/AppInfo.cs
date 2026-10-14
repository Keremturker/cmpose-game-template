namespace Kturker.UnityBase.Core.Domain
{
    public sealed class AppInfo
    {
        public AppInfo(string productName, string version, string platform)
        {
            ProductName = productName;
            Version = version;
            Platform = platform;
        }

        public string ProductName { get; }
        public string Version { get; }
        public string Platform { get; }
    }
}
