using System;

namespace Kturker.UnityBase.Core.Update
{
    public sealed class SystemClock : IClock
    {
        public long NowMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
