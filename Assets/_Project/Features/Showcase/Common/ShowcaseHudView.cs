using System.Globalization;
using Kturker.UnityBase.Core.Localization;
using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Common
{
    // The score at the top and the game's hint line until the run starts (SceneBuilder builds it in each game scene)
    public sealed class ShowcaseHudView : MonoBehaviour, IShowcaseHudView
    {
        [SerializeField] private LocalizedText score;
        [SerializeField] private GameObject hint;

        private int shown = -1;

        public void ShowScore(int value)
        {
            if (value == shown) return;
            shown = value;
            score.SetLiteral(value.ToString(CultureInfo.InvariantCulture));
        }

        public void ShowHint(bool visible) => hint.SetActive(visible);
    }
}
