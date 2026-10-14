using System;

namespace Kturker.UnityBase.Core.Network
{
    // Either a value or a NetworkError; a cancelled call is a failure too (Cancelled)
    public sealed class ApiResult<T>
    {
        private ApiResult(T value, NetworkError error)
        {
            Value = value;
            Error = error;
        }

        public bool IsSuccess => Error == null;
        public T Value { get; }
        public NetworkError Error { get; }

        public static ApiResult<T> Success(T value) => new ApiResult<T>(value, null);

        public static ApiResult<T> Failure(NetworkError error) =>
            new ApiResult<T>(default, error ?? throw new ArgumentNullException(nameof(error)));
    }
}
