namespace Kturker.UnityBase.Features.Showcase.Common
{
    // A showcase game's HUD: the score at the top and the game's hint until the run starts
    public interface IShowcaseHudView
    {
        void ShowScore(int score);
        void ShowHint(bool visible);
    }
}
