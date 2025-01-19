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
        
        internal bool EditMode { get;set; }
        public GameObject interactablesParent;
        private List<GameObject> interactables;
        public GameObject radialMenu;
        private GeneralUIController generalUIController;
        public GameObject colorPalette;
        private RuleEngine _ruleEngine;
        public List<GameObject> ecaObjectEditOptions;
        
       
        private void Start()
        {
            EditMode = false;
            generalUIController = this.gameObject.GetComponent<GeneralUIController>();
            radialMenu.SetActive(true);
            _ruleEngine = RuleEngine.GetInstance();
        }


        public void ActivateEditMode()
        {
            EditMode = true;
            UpdateInteractablesList();
            AddListenerToInteractables();
            generalUIController.EditModeState();
        }
        
        
        public void DeActivateEditMode()
        {
            EditMode = false;
            UpdateInteractablesList();
            RemoveListenerToInteractables();
            generalUIController.SetSelectedObject(null);
            radialMenu.SetActive(true);
            colorPalette.SetActive(false);
            generalUIController.resetEditButtons();
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
        
        public void ShowEcaObjectOptions()
        {
            foreach (var button in ecaObjectEditOptions)
            {
                button.SetActive(true);
            }
            
            // set the parent of one of the buttons to active
            ecaObjectEditOptions[0].transform.parent.gameObject.SetActive(true);
        }
        
        public void RemoveListenerToInteractables()
        {
            if(interactables.Count==0) return;
            foreach (var interactable in interactables)
            {
                //Test 
                if (generalUIController.test)
                {
                    if (interactable.name.Equals("Door") || interactable.name.Equals("Door_doorway")
                                                         || interactable.name.Equals("Old_Door_Closed")) break; 
                }
                
                ObjectManipulator _objectManipulator = interactable.GetComponent<ObjectManipulator>();
                _objectManipulator.OnClicked.RemoveAllListeners();
            }
        }

        private void UpdateInteractablesList()
        {
            interactables = (from Transform child in interactablesParent.transform select child.gameObject).ToList();
        }
        
        public void ShowHideRadialMenu(bool visibility)
        {
            radialMenu.SetActive(visibility);
        }

        /*
         * Since the object is instantiated a runtime, we need to add the listener to each button of the color palette
         * to change the right selected object
         */
        /*public void ChangeColorEventListener()
        {
            if (selectedObject == null) return;
            
            foreach (var color in colors)
            {
                
                PressableButton button = color.GetComponent<PressableButton>();
                button.OnClicked.AddListener(() => selectedObject.GetComponent<ECAObject>().ChangeColor(color.name));
                
            }
        }
        
        public void RemoveColorEventListener()
        {
            if(selectedObject==null) return;
            foreach (var color in colors)
            {
                PressableButton button = color.GetComponent<PressableButton>();
                button.OnClicked.RemoveAllListeners();
            }
        }*/
        
        
        public void addAccessories(String accessory)
        {
            if (generalUIController.GetSelectedObject().GetComponent<ECACharacter>() != null)
            {
                GameObject accessoryObject = generalUIController.GetSelectedObject().transform.Find(accessory).gameObject;
                if (!accessoryObject.activeInHierarchy)
                {
                    accessoryObject.SetActive(true);
                }
            }
        }
        
        public void removeAccessories(String accessory)
        {
            if (generalUIController.GetSelectedObject().GetComponent<ECACharacter>() != null)
            {
                GameObject accessoryObject = generalUIController.GetSelectedObject().transform.Find(accessory).gameObject;
                if (accessoryObject.activeInHierarchy)
                {
                    accessoryObject.SetActive(false);
                }
            }
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