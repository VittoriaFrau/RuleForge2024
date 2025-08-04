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
                if (!Utils.IsEquipable(collision.gameObject))
                {
                    if (GeneralUIController.Instance.UIstate == GeneralUIController.UIState.Play)
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