using ECAPrototyping.RuleEngine;

namespace UI
{
    using UnityEngine;
    using System;

    public class ProximityTriggerListener : MonoBehaviour
    {
        // Public event that other classes can subscribe to
        public event Action<GameObject> OnProximityEnter;
        
        public void ClearListeners()
        {
            OnProximityEnter = null;
        }

        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Interactable"))
            {
                if (GeneralUIController.Instance.UIstate == GeneralUIController.UIState.Play)
                {
                    ECAEvent proximityEcaEvent = GeneralUIController.Instance.recordedEvents
                        .Find(e => e.Modality == InteractionCreationController.Modalities.Proximity && 
                                   Utils.HaveSameRoot(collision.gameObject.name, e.ObjectStr));
                    // print all recorded events
                    GeneralUIController.Instance.recordedEvents
                        .ForEach(e => Debug.Log($"Recorded Event: {e.EventStr} on {e.ObjectStr} with modality {e.Modality}"));
                    if (proximityEcaEvent == null)
                    {
                        if (GeneralUIController.Instance.CombineRulesController.lastECAEvent.Modality ==
                            InteractionCreationController.Modalities.Proximity)
                        {
                            proximityEcaEvent = GeneralUIController.Instance.CombineRulesController.lastECAEvent;
                        }
                    }
                    
                    var trackerDict = GeneralUIController.Instance.CombineRulesController.EventTrackers;
                    if (trackerDict.TryGetValue(proximityEcaEvent, out var tracker))
                    {
                        OnProximityEnter = (other) =>
                        {
                            Debug.Log($"Proximity detected with {other.name}, triggering correct tracker.");
                            tracker.EventTriggered(proximityEcaEvent);
                        };
                    }
                    else
                    {
                        Debug.LogWarning("No tracker found for proximity event.");
                    }

                    string objectNameOfLastEcaEvent = proximityEcaEvent.ObjectStr;
                    string subjectNameOfLastEcaEvent = proximityEcaEvent.Subject;
                    if (Utils.HaveSameRoot(collision.gameObject.name, objectNameOfLastEcaEvent) ||
                        Utils.HaveSameRoot(collision.gameObject.name, subjectNameOfLastEcaEvent))
                    {
                        GeneralUIController.Instance.InteractionCreationController.LastTriggeredObjects.Add(gameObject);
                        GeneralUIController.Instance.InteractionCreationController.LastTriggeredObjects.Add(collision.gameObject);
                    }
                    OnProximityEnter?.Invoke(collision.gameObject);
                }
            }
            
        }
    }
}