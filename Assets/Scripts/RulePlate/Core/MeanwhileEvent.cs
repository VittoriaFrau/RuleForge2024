using UnityEngine;

namespace UI.RuleEditor
{
    /// <summary>
    /// A set of events belonging to a merged (parallel) cube. Thin Unity-facing wrapper around
    /// <see cref="ParallelWindowTracker"/>, which holds the actual window logic.
    /// </summary>
    public class MeanwhileEvent
    {
        public ECAEvent[] events;
        public int CubeID;

        private readonly ParallelWindowTracker tracker;

        public bool IsComplete => tracker.IsComplete;

        public MeanwhileEvent(ECAEvent[] events) : this(events, RuleTimingConfig.ParallelWindowSeconds)
        {
        }

        public MeanwhileEvent(ECAEvent[] events, float windowSeconds)
        {
            this.events = events;
            tracker = new ParallelWindowTracker(events, windowSeconds);
        }

        public void Reset()
        {
            tracker.Reset();
        }

        public bool HasTriggered(ECAEvent ecaEvent)
        {
            return tracker.HasTriggered(ecaEvent);
        }

        /// <summary>
        /// Registers that <paramref name="ecaEvent"/> fired now. Returns true if the window is
        /// complete (every event of this merged cube has fired inside the current window).
        /// </summary>
        public bool RegisterTrigger(ECAEvent ecaEvent)
        {
            return RegisterTrigger(ecaEvent, Time.time);
        }

        public bool RegisterTrigger(ECAEvent ecaEvent, float now)
        {
            return tracker.RegisterTrigger(ecaEvent, now);
        }
    }
}
