namespace Kturker.UnityBase.Features.Sample.Presentation
{
    // How many times the sample was completed; null when nothing is counted (no save system)
    public interface ICompletionCounter
    {
        int? Current { get; }
        int? Record();
    }
}
