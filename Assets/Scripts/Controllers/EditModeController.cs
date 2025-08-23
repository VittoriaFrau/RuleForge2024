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
        private RuleEngine _ruleEngine;
       
        private void Start()
        {
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
            GeneralUIController.Instance.SetSelectedObject(null);
        }
        
        private void AddListenerToInteractables()
        {
            foreach (GameObject interactable in interactables)
            {
                if (!interactable.name.Contains("spawnCube"))
                {
                    AddListenerToSingleInteractable(interactable);
                }
            }
        }

        public void AddListenerToSingleInteractable(GameObject interactable)
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
            _objectManipulator.OnClicked.AddListener(() =>
            {
                interactable.GetComponent<Prototypation>().ShowEditMenu();
                // if it's equipable, freeze all
                if(Utils.IsEquipable(interactable))
                    _objectManipulator.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            });
        }
        
        public void RemoveListenerToInteractables()
        {
            if(interactables.Count==0) return;
            foreach (var interactable in interactables)
            {
                ObjectManipulator _objectManipulator = interactable.GetComponent<ObjectManipulator>();
                if(_objectManipulator != null) 
                    _objectManipulator.OnClicked.RemoveAllListeners();
                Prototypation _prototypation = interactable.GetComponent<Prototypation>();
                if (_prototypation != null)
                {
                    // Remove the Prototypation component if it exists
                    Destroy(_prototypation);
                }
            }
        }

        private void UpdateInteractablesList()
        {
            interactables = (from Transform child in interactablesParent.transform select child.gameObject).ToList();
        }
        
        //Method for Unity to create and publish an action
        public void CreateAndPublishAction(string actionName)
        {
            CreateAndPublishAction(actionName, null);
        }

        public void CreateAndPublishAction(string actionName, Action action = null)
        {
            if (_ruleEngine == null) return;
            if(action==null) 
                action = Utils.GetActionFromString(actionName,  GeneralUIController.Instance.GetSelectedObject());
            // If I'm recording, I need to save the action
            if (GeneralUIController.Instance.isRecording && !actionName.Contains("moves"))
            {
                GeneralUIController.Instance.InteractionCreationController.SaveRecordedAction(action);
            }else if (actionName.Contains("moves"))
            {
                // we need to assign to the action the proximity object
                GameObject proximityObject =  GeneralUIController.Instance.InteractionCreationController.spawnCube
                    .GetComponentInChildren<SpawnCubeCollision>(true).ProximityGameObject1;
                action.SetObject(proximityObject);
            }

            switch (GeneralUIController.Instance.CategoryController.lastCategorySelected)
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