using ECAPrototyping.RuleEngine;
using UnityEngine;

namespace UI
{
    public class EventSequenceTracker
    {
        public int CurrentIndex { get; private set; } = 0;
        private readonly ECAEvent[] sequence;
        private readonly ECAEvent[] thenEvents;
        private readonly RuleEngine ruleEngine;

        public EventSequenceTracker(ECAEvent[] whenSequence, ECAEvent[] thenEvents, RuleEngine ruleEngine)
        {
            this.sequence = whenSequence;
            this.thenEvents = thenEvents;
            this.ruleEngine = ruleEngine;
        }

        public void EventTriggered(ECAEvent triggeredEvent)
        {
            if (sequence.Length == 0) return;

            var expectedEvent = sequence[CurrentIndex];

            if (triggeredEvent == expectedEvent)
            {
                Debug.Log($"[Tracker] Event {CurrentIndex} ({triggeredEvent.EventStr}) triggered correctly on {triggeredEvent.ObjectRef.name}");

                CurrentIndex++;

                if (CurrentIndex >= sequence.Length)
                {
                    Debug.Log($"[Tracker] Sequence complete! Executing actions.");
                    foreach (var thenEvent in thenEvents)
                    {
                        ruleEngine.ExecuteAction(thenEvent.Action);
                    }

                    CurrentIndex = 0; // resetta se vuoi che la sequenza si ripeta
                }
            }
            else
            {
                Debug.Log($"[Tracker] Unexpected event '{triggeredEvent.EventStr}'. Resetting sequence.");
                CurrentIndex = 0;
            }
        }
    }

}