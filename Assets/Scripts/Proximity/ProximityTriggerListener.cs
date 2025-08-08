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
                    string objectNameOfLastEcaEvent = GeneralUIController.Instance.CombineRulesController.lastECAEvent.ObjectStr;
                    string subjectNameOfLastEcaEvent =
                        GeneralUIController.Instance.CombineRulesController.lastECAEvent.Subject;
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