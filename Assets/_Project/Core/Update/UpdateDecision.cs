using System;

namespace Kturker.UnityBase.Core.Update
{
    // StoreUrl: the App Store page on iOS; null on Android, where Update goes through Google Play
    public sealed class UpdateDecision : IEquatable<UpdateDecision>
    {
        public static readonly UpdateDecision None = new UpdateDecision(UpdateKind.None, null);

        private UpdateDecision(UpdateKind kind, string storeUrl)
        {
            Kind = kind;
            StoreUrl = storeUrl;
        }

        public UpdateKind Kind { get; }
        public string StoreUrl { get; }

        public static UpdateDecision Available(string storeUrl) => new UpdateDecision(UpdateKind.Available, storeUrl);

        public static UpdateDecision Required(string storeUrl) => new UpdateDecision(UpdateKind.Required, storeUrl);

        public bool Equals(UpdateDecision other) => other != null && other.Kind == Kind && other.StoreUrl == StoreUrl;

        public override bool Equals(object obj) => Equals(obj as UpdateDecision);

        public override int GetHashCode() => ((int)Kind * 397) ^ (StoreUrl != null ? StoreUrl.GetHashCode() : 0);

        public override string ToString() => StoreUrl == null ? Kind.ToString() : $"{Kind} {StoreUrl}";
    }
}
