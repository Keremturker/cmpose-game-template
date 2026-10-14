using UnityEngine;

namespace Kturker.UnityBase.Core.Presentation
{
    // The Game scene's column where module panels stack; each module's installer spawns its panel into Content
    public sealed class ModuleSlot : MonoBehaviour
    {
        [SerializeField] private RectTransform content;

        public RectTransform Content => content;
    }
}
