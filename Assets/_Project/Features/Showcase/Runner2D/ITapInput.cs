namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    public interface ITapInput
    {
        // Still touched
        bool Held { get; }

        // True once per touch: the runner takes it in the next frame
        bool ConsumePress();
    }
}
