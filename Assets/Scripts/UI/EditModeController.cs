using System;
using System.Collections.Generic;
using System.Linq;
using ECAPrototyping.RuleEngine;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Action = ECAPrototyping.RuleEngine.Action;
using RenderSettings = UnityEngine.RenderSettings;

namespace UI
{
    public class EditModeController : MonoBehaviour
    {
        
        public GameObject interactablesParent;
        private List<GameObject> interactables;
        private GeneralUIController generalUIController;
        private RuleEngine _ruleEngine;
       
        private void Start()
        {
            generalUIController = this.gameObject.GetComponent<GeneralUIController>();
            _ruleEngine = RuleEngine.GetInstance();
        }
        
        public void UpdateAndAddListeners()
        {
            UpdateInteractablesList();
            AddListenerToInteractables();
        }
        
        public void UpdateAndRemoveListeners()
        {
            UpdateInteractablesList();
            RemoveListenerToInteractables();
            generalUIController.SetSelectedObject(null);
        }
        
        private void AddListenerToInteractables()
        {
            foreach (var interactable in interactables)
            {
                ObjectManipulator _objectManipulator = interactable.GetComponent<ObjectManipulator>();
                if (_objectManipulator == null)
                {
                    _objectManipulator = interactable.GetComponentInChildren<ObjectManipulator>();
                }
                
                // Check if the Prototypation component is attached to the interactable object, if not, add it
                if (interactable.GetComponent<Prototypation>() == null)
                {
                    interactable.AddComponent<Prototypation>();
                }
                _objectManipulator.OnClicked.AddListener(() => interactable.GetComponent<Prototypation>().ShowEditMenu());
            }
        }
        
        public void RemoveListenerToInteractables()
        {
            if(interactables.Count==0) return;
            foreach (var interactable in interactables)
            {
                ObjectManipulator _objectManipulator = interactable.GetComponent<ObjectManipulator>();
                _objectManipulator.OnClicked.RemoveAllListeners();
            }
        }

        private void UpdateInteractablesList()
        {
            interactables = (from Transform child in interactablesParent.transform select child.gameObject).ToList();
        }

        public void CreateAndPublishAction(string actionName)
        {
            if (_ruleEngine == null) return;
            Action action = Utils.GetActionFromString(actionName, generalUIController.GetSelectedObject());
            _ruleEngine.ExecuteAction(action);
            
            // If I'm recording, I need to save the action
            if (generalUIController.isRecording)
            {
                generalUIController.InteractionCreationController.SaveRecordedAction(action);
            }

        }

    }
}