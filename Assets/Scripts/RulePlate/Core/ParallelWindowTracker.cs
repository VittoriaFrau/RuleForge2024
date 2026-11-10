using System.Collections.Generic;

namespace UI.RuleEditor
{
    /// <summary>
    /// Tracks whether a fixed set of events all occur close enough together in time to be
    /// considered "parallel" (paper: 200-300 ms timeout for parallel inputs on merged cubes).
    /// Plain C# logic, no Unity dependency: the current time is always passed in by the caller.
    /// </summary>
    public class ParallelWindowTracker
    {
        private readonly ECAEvent[] events;
        private readonly float windowSeconds;
        private readonly HashSet<ECAEvent> triggered = new HashSet<ECAEvent>();
        private float? windowStartTime;

        public ParallelWindowTracker(ECAEvent[] events, float windowSeconds)
        {
            this.events = events;
            this.windowSeconds = windowSeconds;
        }

        public bool IsComplete => triggered.Count >= events.Length;

        public bool HasTriggered(ECAEvent ecaEvent) => triggered.Contains(ecaEvent);

        /// <summary>
        /// Registers that <paramref name="ecaEvent"/> fired at <paramref name="now"/>.
        /// Opens a new window if none is open, or if the previous one expired (in which case
        /// previously collected events are discarded). Returns true if, after this call, every
        /// event of the set has been collected inside the current window.
        /// </summary>
        public bool RegisterTrigger(ECAEvent ecaEvent, float now)
        {
            if (!windowStartTime.HasValue || now - windowStartTime.Value > windowSeconds)
            {
                triggered.Clear();
                windowStartTime = now;
            }

            triggered.Add(ecaEvent);

            bool complete = IsComplete;
            if (complete) Reset();
            return complete;
        }

        public void Reset()
        {
            triggered.Clear();
            windowStartTime = null;
        }
    }
}
