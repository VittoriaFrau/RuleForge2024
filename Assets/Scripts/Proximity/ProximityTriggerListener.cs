using ECAPrototyping.RuleEngine;

namespace UI
{
    using UnityEngine;
    using System;

    public class ProximityTriggerListener : MonoBehaviour
    {
        // Public event that other classes can subscribe to
        public event Action<GameObject> OnProximityEnter;
        public GameObject collidedGameObject;

       /* private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"OnTriggerEnter detected on {gameObject.name} with {other.name}");

            OnProximityEnter?.Invoke(other);
        }*/
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Interactable"))
            {
                //DEMO: to undemo remove the comment from the next line
                //OnProximityEnter?.Invoke(collision.gameObject);
            
                //DEMO
                if (collision.gameObject.name.Contains("Rock"))
                {
                    Debug.Log($"Collision detected with {collision.gameObject.name}");
                    collidedGameObject = collision.gameObject;
                    RuleEngine ruleEngine = RuleEngine.singleton;
                    if (ruleEngine != null)
                    {
                        ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(collidedGameObject, "explodes"));
                        ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Counter1"), "increases by one"));
                        ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(this.gameObject, "explodes"));
                    }
                }
            }
            
        }
    }
}