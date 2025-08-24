using ECAPrototyping.RuleEngine;
using TMPro;

namespace UI
{
    using UnityEngine;
    using System;
    using System.Linq;

    public class ProximityTriggerListener : MonoBehaviour
    {

        void Start()
        {
        }
        // Public event that other classes can subscribe to
        public event Action<GameObject> OnProximityEnter;
        
        public void ClearListeners()
        {
            OnProximityEnter = null;
        }

        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Interactable") || collision.gameObject.CompareTag("Floor"))
            {
                if (GeneralUIController.Instance.UIstate == GeneralUIController.UIState.Play)
                {
                    /*ECAEvent proximityEcaEvent = GeneralUIController.Instance.recordedEvents
                        .Find(e => e.Modality == InteractionCreationController.Modalities.Proximity &&
                                   Utils.HaveSameRoot(this.gameObject.name, e.ObjectRef.name) && Utils.HaveSameRoot(e.Subject, collision.gameObject.name));}
                    if (proximityEcaEvent == null)
                    {
                        Debug.Log("No proximity event found");
                        return;
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
                    OnProximityEnter?.Invoke(collision.gameObject);*/

                    if (gameObject.name.ToLower().Contains("cube"))
                    {
                        RuleEngine ruleEngine = RuleEngine.GetInstance();
                        if (collision.gameObject.name.ToLower().Contains("sword") )
                        {

                            ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(gameObject, "explodes"));
                            /*TextMeshPro textMeshPro = GameObject.Find("Counter1").GetComponent<TextMeshPro>();
                            int counterValue = Convert.ToInt32(textMeshPro.text);
                            textMeshPro.text = (counterValue + 1).ToString();
                            //GeneralUIController.Instance.CombineRulesController.manualOppositeActions.Add(new ECAPrototyping.RuleEngine.Action(gameObject, "resets"));
                            GeneralUIController.Instance.CombineRulesController.manualOppositeActions.Add(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Counter1"), "resets counter"));
                            done = true;*/

                        }
                        else if (collision.gameObject.name.ToLower().Contains("floor") )
                        {

                            GameObject belt = GameObject.FindGameObjectsWithTag("Interactable").FirstOrDefault(obj => obj.name.Equals("Belt1"));
                            if (belt != null)
                            {
                                ruleEngine.ExecuteAction(new ECAPrototyping.RuleEngine.Action(gameObject, "hides"));
                                /*TextMeshPro textMeshPro = GameObject.Find("Counter1").GetComponent<TextMeshPro>();
                                int counterValue = Convert.ToInt32(textMeshPro.text);
                                textMeshPro.text = (counterValue - 1).ToString();
                                GeneralUIController.Instance.CombineRulesController.manualOppositeActions.Add(new ECAPrototyping.RuleEngine.Action(GameObject.Find("Counter1"), "resets counter"));
                                done = true;*/
                            }
                        }

                        else if (collision.gameObject.name.ToLower().Contains("belt"))
                        {
                            UI.Utils.Demo(gameObject);
                        }
                    }
            }
            }
            
        }
    }
}