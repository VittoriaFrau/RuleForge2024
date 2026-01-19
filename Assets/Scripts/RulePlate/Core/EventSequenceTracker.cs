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
        private readonly ECAEvent[] ifEvents;
        private readonly ECAEvent[] thenEvents;
        private readonly RuleEngine ruleEngine;
        private List<Action> oppositeActions;
        private bool hasCompleted = false;

        public EventSequenceTracker(ECAEvent[] whenSequence, ECAEvent[] ifEvents, ECAEvent[] thenEvents, RuleEngine ruleEngine)
        {
            this.sequence = whenSequence;
            this.ifEvents = ifEvents;
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

        /*private void ExecuteActions()
        {
            foreach (var thenEvent in thenEvents)
            {
                if (thenEvent.Action.GetSubject() == null) // for instance when duplicating the gameobject does not exist yet
                {
                    thenEvent.Action.SetSubject(GameObject.Find(thenEvent.Subject));
                }

                if (thenEvent.Action.GetSubject().GetComponent<ECAObject>().isACopy)
                {
                    Debug.Log("Action subject is duplicated, we need to find the newest instance.");
                    thenEvent.Action.SetSubject(GeneralUIController.Instance.ObjectsMenuController.spawnedObjects.FindLast(x=>
                        x.name.Contains(thenEvent.Action.GetSubject().name)));
                    Debug.Log($"New subject for action: {thenEvent.Action.GetSubject().name}");
                }
                ruleEngine.ExecuteAction(thenEvent.Action);
                oppositeActions.Add(Utils.GetOppositeAction(thenEvent.Action, thenEvent.Verb));
            }

            hasCompleted = true;
            CurrentIndex = 0;

            // Reset so the rule can be triggered again
            ResetTracker();
        }*/
        
        private void ExecuteActions()
        {
            Debug.Log($"[Tracker] ExecuteActions: ifEvents={ifEvents?.Length ?? 0}, thenEvents={thenEvents?.Length ?? 0}");
            if (!AreIfConditionsMet(ifEvents))
            {
                GeneralUIController.Instance.SetDebugText("IF condition not satisfied. THEN skipped.");
                Debug.Log("[Tracker] IF condition not satisfied. THEN skipped.");
                ResetTracker();
                return;
            }

            hasCompleted = true;
            CurrentIndex = 0;

            GameObject latestDuplicate = null;
            string baseNameForDuplicates = "";

            DetectBaseNameForDuplicates(ref baseNameForDuplicates);
            ExecuteThenEvents(ref latestDuplicate, baseNameForDuplicates);

            ResetTracker();
        }

        private void DetectBaseNameForDuplicates(ref string baseNameForDuplicates)
        {
            foreach (var thenEvent in thenEvents)
            {
                var action = thenEvent.Action;
                var subject = action.GetSubject();

                if (thenEvent.Verb == "is duplicated" &&  action.GetSubject() != null)
                {
                    baseNameForDuplicates = subject.name.Substring(0, subject.name.Length - 1);
                }
            }
        }

        private void ExecuteThenEvents(ref GameObject latestDuplicate, string baseNameForDuplicates)
        {
            foreach (var thenEvent in thenEvents)
            {
                var action = thenEvent.Action;
                var subject = action.GetSubject();
                var lastTriggeredObjects = GeneralUIController.Instance.InteractionCreationController.LastTriggeredObjects;
                
                if (action.IsDynamicSubject)
                {
                    // we need to find the last triggered object using the list
                    string ecaCategory = Utils.GetECALastScriptFromECAObject(action.GetSubject());
                    foreach (var ob in lastTriggeredObjects)
                    {
                        string obCategory = Utils.GetECALastScriptFromECAObject(ob);
                        if(obCategory.Equals(ecaCategory))
                        {
                            action.SetSubject(ob);
                            // remove from the list so it is not used again
                            lastTriggeredObjects.Remove(ob);
                            Debug.Log($"[Tracker] Assigned dynamic subject {ob.name} to action {thenEvent.Verb}");
                            break;
                        }
                    }
                    // Clear the list
                    lastTriggeredObjects.Clear();
                }

                if (thenEvent.Verb == "is duplicated")
                {
                    ruleEngine.ExecuteAction(action);
                    //if opposite action is not already in the list, we add it
                    if (!oppositeActions.Contains(Utils.GetOppositeAction(action, thenEvent.Verb)))
                        oppositeActions.Add(Utils.GetOppositeAction(action, thenEvent.Verb));
                    latestDuplicate = FindLatestDuplicate(baseNameForDuplicates);
                    continue;
                }

                // only assign if subject is missing or marked as a duplicate
                if (subject == null || subject.GetComponent<ECAObject>().isACopy)
                {
                    if (latestDuplicate != null)
                    {
                        action.SetSubject(latestDuplicate);
                        subject = latestDuplicate;
                    }
                }
    
                //DEMO. if not needed, remove this block and leave the execute action only
                if (thenEvent.Verb.Contains("is duplicated"))
                {
                    
                }
                else ruleEngine.ExecuteAction(action);
                // the duplicates are not considered in the opposite actions because they will be destroyed 
                //if the subject is not a duplicate, we can add the opposite action
                // or if the UIState is in edit mode, in play we delete
                if (!subject.GetComponent<ECAObject>().isACopy || 
                    GeneralUIController.Instance.UIstate == GeneralUIController.UIState.EditMode) 
                {
                    if (!oppositeActions.Contains(Utils.GetOppositeAction(action, thenEvent.Verb)))
                        oppositeActions.Add(Utils.GetOppositeAction(action, thenEvent.Verb));
                }
            }
        }

        private GameObject FindLatestDuplicate(string baseName)
        {
            return GeneralUIController.Instance.ObjectsMenuController.spawnedObjects
                .FindLast(obj => obj.name.StartsWith("new" + baseName));
        }



        
        public void ExecuteMeanwhileAction(MeanwhileEvent @event)
        {
            // Azione: scateni le azioni di THEN
            // Ad esempio:
            EventSequenceTracker tracker = new EventSequenceTracker(new ECAEvent[0], ifEvents, thenEvents, RuleEngine.GetInstance());
            tracker.TriggerActionsDirectly(null); // oppure una logica più complessa
        }
        
        
        public void ExecuteOppositeActions()
        {
            Debug.Log("[Tracker] Executing opposite actions...");
            //log the opposite actions
            foreach (var action in oppositeActions)
            {
                Debug.Log($"[Tracker] Opposite Action: " + action.ToString());
            }
            if (oppositeActions == null || oppositeActions.Count == 0) return;

            foreach (var action in oppositeActions)
            {
                ruleEngine.ExecuteAction(action);
            }

            oppositeActions.Clear();
        }
        
        public void ResetTracker()
        {
            hasCompleted = false;
            CurrentIndex = 0;
            //oppositeActions.Clear(); 
        }

        public static bool AreIfConditionsMet(ECAEvent[] ifEvents)
        {
            if (ifEvents == null || ifEvents.Length == 0)
            {
                return true;
            }

            foreach (var ifEvent in ifEvents)
            {
                if (ifEvent == null)
                {
                    return false;
                }

                var action = ifEvent.Action;
                var subject = action != null ? action.GetSubject() : ifEvent.ObjectRef;
                if (subject == null)
                {
                    Debug.LogWarning("[Tracker] IF condition has no subject.");
                    return false;
                }

                var ecaObject = subject.GetComponent<ECAPrototyping.RuleEngine.ECAObject>();
                if (ecaObject == null)
                {
                    Debug.LogWarning($"[Tracker] IF subject '{subject.name}' has no ECAObject.");
                    return false;
                }

                if (!TryEvaluateVisibility(ifEvent, ecaObject, out var matches))
                {
                    Debug.LogWarning($"[Tracker] IF condition not supported: {ifEvent.Verb}");
                    return false;
                }

                if (!matches)
                {
                    Debug.Log($"[Tracker] IF condition failed for {subject.name}: {ifEvent.Verb}");
                    return false;
                }
            }

            return true;
        }

        private static bool TryEvaluateVisibility(ECAEvent ifEvent, ECAPrototyping.RuleEngine.ECAObject ecaObject, out bool matches)
        {
            matches = false;
            var verb = (ifEvent.Verb ?? ifEvent.Action?.GetActionMethod() ?? string.Empty).ToLowerInvariant();

            if (verb.Contains("hides"))
            {
                matches = !ecaObject.isVisible;
                return true;
            }

            if (verb.Contains("shows"))
            {
                matches = ecaObject.isVisible;
                return true;
            }

            if (verb.Contains("changes visible to") || verb.Contains("changes visibility to"))
            {
                object modifierValue = ifEvent.Action != null ? ifEvent.Action.GetModifierValue() : null;
                if (modifierValue == null)
                {
                    modifierValue = ifEvent.ObjectStr;
                }

                if (TryParseBool(modifierValue, out var desiredVisible))
                {
                    matches = ecaObject.isVisible == desiredVisible;
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseBool(object value, out bool result)
        {
            result = false;
            if (value == null) return false;

            if (value is bool boolVal)
            {
                result = boolVal;
                return true;
            }

            if (value is ECAPrototyping.Utils.ECABoolean ecaBool)
            {
                result = ecaBool;
                return true;
            }

            var text = value.ToString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(text)) return false;

            if (text is "yes" or "true" or "on")
            {
                result = true;
                return true;
            }

            if (text is "no" or "false" or "off")
            {
                result = false;
                return true;
            }

            return bool.TryParse(text, out result);
        }

    }


}
