namespace Kturker.UnityBase.Core.Purchases
{
    public sealed class StoreReply<T>
    {
        private StoreReply(bool ok, T value, StoreErrorKind error)
        {
            IsOk = ok;
            Value = value;
            Error = error;
        }

        public bool IsOk { get; }
        public T Value { get; }
        public StoreErrorKind Error { get; }

        public static StoreReply<T> Ok(T value) => new StoreReply<T>(true, value, StoreErrorKind.Other);

        public static StoreReply<T> Failed(StoreErrorKind error) => new StoreReply<T>(false, default, error);
    }
}
