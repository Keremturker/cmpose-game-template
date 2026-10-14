namespace Kturker.UnityBase.Core.Update
{
    public interface IClock
    {
        // Unix time in milliseconds
        long NowMillis { get; }
    }
}
