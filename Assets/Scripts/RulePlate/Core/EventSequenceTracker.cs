using System.Collections.Generic;
using ECAPrototyping.RuleEngine;
using UI.RuleEditor;
using UnityEngine;

namespace UI
{
    public class EventSequenceTracker
    {
        public int CurrentIndex { get; private set; } = 0;
        private readonly ECAEvent[] sequence;
        private readonly ECAEvent[] thenEvents;
        private readonly RuleEngine ruleEngine;
        private readonly List<Action> oppositeActions;
        private bool hasCompleted = false;

        public EventSequenceTracker(ECAEvent[] whenSequence, ECAEvent[] thenEvents, RuleEngine ruleEngine)
        {
            this.sequence = whenSequence;
            this.thenEvents = thenEvents;
            this.ruleEngine = ruleEngine;
            this.oppositeActions = new List<Action>();
        }

        public void EventTriggered(ECAEvent triggeredEvent)
        {
            if (hasCompleted) return;

            if (sequence.Length == 0) return;

            var expectedEvent = sequence[CurrentIndex];

            if (triggeredEvent == expectedEvent)
            {
                Debug.Log($"[Tracker] Event {CurrentIndex} ({triggeredEvent.EventStr}) triggered correctly on {triggeredEvent.ObjectRef?.name ?? "null object"}");

                CurrentIndex++;

                if (CurrentIndex >= sequence.Length)
                {
                    Debug.Log($"[Tracker] Sequence complete! Executing actions.");
                    ExecuteActions();
                }
            }
            else
            {
                Debug.Log($"[Tracker] Unexpected event '{triggeredEvent.EventStr}'. Resetting sequence.");
                CurrentIndex = 0;
            }
        }

        public void TriggerActionsDirectly(ECAEvent equivalenceEvent)
        {
            if (hasCompleted) return;

            Debug.Log($"[Tracker] Equivalence event '{equivalenceEvent.EventStr}' triggered! Executing actions immediately.");
            ExecuteActions();
        }

        private void ExecuteActions()
        {
            foreach (var thenEvent in thenEvents)
            {
                if (thenEvent.Action.GetSubject() == null) // for instance when duplicating the gameobject does not exist yet
                {
                    thenEvent.Action.SetSubject(GameObject.Find(thenEvent.Subject));
                }
                ruleEngine.ExecuteAction(thenEvent.Action);
                oppositeActions.Add(Utils.GetOppositeAction(thenEvent.Action, thenEvent.Verb));
            }

            hasCompleted = true;
            CurrentIndex = 0;
        }
        
        public void ExecuteMeanwhileAction(MeanwhileRule rule)
        {
            // Azione: scateni le azioni di THEN
            // Ad esempio:
            EventSequenceTracker tracker = new EventSequenceTracker(new ECAEvent[0], thenEvents, RuleEngine.GetInstance());
            tracker.TriggerActionsDirectly(null); // oppure una logica più complessa
        }
        
        
        public void ExecuteOppositeActions()
        {
            if (oppositeActions == null || oppositeActions.Count == 0) return;

            foreach (var action in oppositeActions)
            {
                ruleEngine.ExecuteAction(action);
            }

            oppositeActions.Clear();
        }
    }


}