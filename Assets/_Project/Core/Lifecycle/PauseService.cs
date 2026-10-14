using System;
using System.Collections.Generic;

namespace Kturker.UnityBase.Core.Lifecycle
{
    // Paused while any reason is set; Paused/Resumed fire only when the whole set turns non-empty/empty
    public sealed class PauseService
    {
        private readonly HashSet<PauseReason> reasons = new HashSet<PauseReason>();

        public event Action Paused;
        public event Action Resumed;

        public bool IsPaused => reasons.Count > 0;

        public bool Has(PauseReason reason) => reasons.Contains(reason);

        public void Pause(PauseReason reason)
        {
            var wasPaused = IsPaused;
            if (!reasons.Add(reason) || wasPaused) return;
            Paused?.Invoke();
        }

        public void Resume(PauseReason reason)
        {
            if (!reasons.Remove(reason) || IsPaused) return;
            Resumed?.Invoke();
        }
    }
}
