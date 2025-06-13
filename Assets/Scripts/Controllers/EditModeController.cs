using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
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
        private CategoryController _categoryController;
        private RuleEngine _ruleEngine;
       
        private void Start()
        {
            generalUIController = GetComponent<GeneralUIController>();
            _categoryController = GetComponent<CategoryController>();
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

        public void CreateAndPublishAction(string actionName, Action action = null)
        {
            if (_ruleEngine == null) return;
            if(action==null) 
                action = Utils.GetActionFromString(actionName, generalUIController.GetSelectedObject());
            // If I'm recording, I need to save the action
            if (GeneralUIController.Instance.isRecording && actionName != "moves")
            {
                generalUIController.InteractionCreationController.SaveRecordedAction(action);
            }

            switch (_categoryController.categoryObjectSelected)
            {
                case CategoryController.CategoryObjectSelected.SingleObject:
                    _ruleEngine.ExecuteAction(action);
                    break;
                case CategoryController.CategoryObjectSelected.Category:
                    Utils.ExecuteActionOnCategory(_ruleEngine, action, interactablesParent);
                    break;
                case CategoryController.CategoryObjectSelected.AllObjects:
                    Utils.ExecuteActionOnAllObjects(_ruleEngine, action, interactablesParent);
                    break;
            }
        }

    }
}