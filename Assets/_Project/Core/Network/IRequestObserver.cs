namespace Kturker.UnityBase.Core.Network
{
    // Sees every exchange of IApiClient (the networkInspector flag registers one); exceptions are logged, not thrown
    public interface IRequestObserver
    {
        void OnCompleted(HttpRequest request, HttpResponse response);
    }
}
