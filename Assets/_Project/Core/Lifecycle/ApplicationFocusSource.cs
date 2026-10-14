using Kturker.UnityBase.Core.Preferences;
using UnityEngine;

namespace Kturker.UnityBase.Core.Lifecycle
{
    // Root prefab component: going to the background or losing focus pauses with PauseReason.Focus (no panel),
    // and going to the background saves preferences. Ignored in the editor, where every click outside the
    // Game view is a focus change.
    public sealed class ApplicationFocusSource : MonoBehaviour
    {
        private PauseService pause;
        private IPreferences preferences;

        public void Bind(PauseService pauseService, IPreferences store)
        {
            pause = pauseService;
            preferences = store;
        }

        private void OnApplicationPause(bool paused) => Apply(!paused, paused);

        private void OnApplicationFocus(bool focused) => Apply(focused, false);

        private void Apply(bool active, bool save)
        {
            if (pause == null || Application.isEditor) return;
            if (save) preferences.Save();
            if (active) pause.Resume(PauseReason.Focus);
            else pause.Pause(PauseReason.Focus);
        }
    }
}
