namespace Kturker.UnityBase.Core.Network
{
    // What went wrong below HTTP; None means a status code and body arrived (any status)
    public enum TransportFailure
    {
        None,
        Connection,
        Timeout,
        Cancelled
    }
}
