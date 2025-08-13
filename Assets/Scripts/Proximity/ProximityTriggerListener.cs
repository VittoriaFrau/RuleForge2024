using ECAPrototyping.RuleEngine;

namespace UI
{
    using UnityEngine;
    using System;

    public class ProximityTriggerListener : MonoBehaviour
    {
        // Public event that other classes can subscribe to
        public event Action<GameObject> OnProximityEnter;
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Interactable"))
            {
                if (GeneralUIController.Instance.UIstate == GeneralUIController.UIState.Play)
                {
                    ECAEvent lastProximityEcaEvent;
                    if (GeneralUIController.Instance.CombineRulesController.lastECAEvent.Modality ==
                        InteractionCreationController.Modalities.Proximity)
                    {
                        lastProximityEcaEvent = GeneralUIController.Instance.CombineRulesController.lastECAEvent;
                    }
                    else
                    {
                        // search among all the events
                        lastProximityEcaEvent = GeneralUIController.Instance.recordedEvents
                            .Find(e => e.Modality == InteractionCreationController.Modalities.Proximity);
                    }
                    string objectNameOfLastEcaEvent = lastProximityEcaEvent.ObjectStr;
                    string subjectNameOfLastEcaEvent = lastProximityEcaEvent.Subject;
                    if (Utils.HaveSameRoot(collision.gameObject.name, objectNameOfLastEcaEvent) ||
                        Utils.HaveSameRoot(collision.gameObject.name, subjectNameOfLastEcaEvent))
                    {
                        GeneralUIController.Instance.InteractionCreationController.LastTriggeredObjects.Add(gameObject);
                        GeneralUIController.Instance.InteractionCreationController.LastTriggeredObjects.Add(collision.gameObject);
                    }
                }
                OnProximityEnter?.Invoke(collision.gameObject);
            }
            
        }
    }
}