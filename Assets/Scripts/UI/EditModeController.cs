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
        private List<GameObject> interactables;
        private GameObject canvas;
        public GameObject radialMenu;
        private GeneralUIController generalUIController;
        public List<GameObject> colors;
        public GameObject colorPalette;
        private GameObject selectedObject;
        private Animator _animator;
        private RuleEngine _ruleEngine;
        public Light _mainlight;
        public GameObject _skybox;
        private Renderer plane;
        public Slider lightSlider, volumeSlider, effectSlider;
        public List<GameObject> sceneButtonsToClose;
        public GameObject SelectedObject
        {
            get => selectedObject;
            set { selectedObject = value; }
        }

        private void Start()
        {
            EditMode = false;
            canvas = GameObject.FindGameObjectWithTag("Canvas");
            if(radialMenu==null) radialMenu = canvas.transform.Find("RadialMenu").gameObject;
            /*text = textGo.GetComponent<TextMeshPro>();*/
            generalUIController = this.gameObject.GetComponent<GeneralUIController>();
            radialMenu.SetActive(true);
            _ruleEngine = RuleEngine.GetInstance();
            plane = GameObject.FindGameObjectWithTag("Plane").GetComponent<Renderer>();
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
            selectedObject = null;
            radialMenu.SetActive(true);
            colorPalette.SetActive(false);
            generalUIController.resetEditButtons();
        }
        
        private void AddListenerToInteractables()
        {
            foreach (var interactable in interactables)
            {
                //Test:
                if (generalUIController.test)
                {
                    if(interactable.name.Equals("Door") || interactable.name.Equals("Door_doorway") 
                                                        || interactable.name.Equals("flame")) return;
                }
                
                ObjectManipulator _objectManipulator = interactable.GetComponent<ObjectManipulator>();
                if (_objectManipulator == null)
                {
                    _objectManipulator = interactable.GetComponentInChildren<ObjectManipulator>();
                }
                //TEST:
                if(generalUIController.test && interactable.name.Equals("Old_Door_Closed"))
                {
                    GameObject[] children = { interactable.transform.GetChild(0).gameObject, interactable.transform.GetChild(1).gameObject };
                    Prototypation prototypationScript = interactable.transform.GetChild(1).gameObject.GetComponent<Prototypation>();;
                    foreach (var child in children)
                    {
                        if (child.name.Equals("Door"))
                        {
                            _objectManipulator = child.GetComponent<ObjectManipulator>();
                            prototypationScript = child.GetComponent<Prototypation>();
                        }
                            
                    }

                    if (_objectManipulator == null)
                    {
                        _objectManipulator = interactable.transform.GetChild(1).gameObject.GetComponent<ObjectManipulator>();
                    }
                    
                    //_objectManipulator.OnClicked.AddListener(() => prototypationScript.ShowPieUIMenu());
                }
                //else _objectManipulator.OnClicked.AddListener(() => interactable.GetComponent<Prototypation>().ShowPieUIMenu());
            }
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
            interactables = (from Transform child in GameObject.Find("Interactables").transform select child.gameObject).ToList();
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
            if (selectedObject.GetComponent<ECACharacter>() != null)
            {
                GameObject accessoryObject = selectedObject.transform.Find(accessory).gameObject;
                if (!accessoryObject.activeInHierarchy)
                {
                    accessoryObject.SetActive(true);
                }
            }
        }
        
        public void removeAccessories(String accessory)
        {
            if (selectedObject.GetComponent<ECACharacter>() != null)
            {
                GameObject accessoryObject = selectedObject.transform.Find(accessory).gameObject;
                if (accessoryObject.activeInHierarchy)
                {
                    accessoryObject.SetActive(false);
                }
            }
        }

        public void CreateAndPublishAction(string actionName)
        {
            if (_ruleEngine == null) return;
            Action action = Utils.GetActionFromString(actionName, SelectedObject, volumeSlider, lightSlider,
                effectSlider, plane.gameObject,
                _skybox, _mainlight);
            _ruleEngine.ExecuteAction(action);
            
            //TEST
            //Azione da fare se sto registrando e non è la porta 
            if (generalUIController.isRecording && !selectedObject.name.Equals("Door") )
            {
                generalUIController.InteractionCreationController.RecordActionPressedButton(action, selectedObject);
            }

        }

    }
}