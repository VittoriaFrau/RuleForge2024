using System;
using System.Collections.Generic;
using ECAPrototyping.RuleEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class GeneralUIController: MonoBehaviour
    {
        private GameObject textGo;
        public GameObject debugWindow;
        public List<GameObject> optionButtons;
        public List<GameObject> creationButtons;
        public List<GameObject> editingButtons;
        public List<GameObject> editingButtonsTMP;
        public GameObject editSceneButton;
        public List<GameObject> ruleButtons;
        public List<GameObject> characterButtons;
        public List<GameObject> musicButtons;
        public List<GameObject> lightButtons;
        public List<GameObject> effectButtons;
        public List<GameObject> editSceneButtons;
        public List<GameObject> doorButtons;
        public List<GameObject> stateDependentButtons;
        public GameObject closeButton;
        public GameObject eventHandler;
        private EditModeController _editModeController;
        private ObjectsMenuController _objectsMenuController;
        private InteractionCreationController _interactionCreationController;
        /*public RadialMenu radialMenu;*/
        private Prototypation _prototypation;
        public bool isRecording = false;
        
        // Test poop just to make it work
        public Boolean test = true;
        

        public enum UIState
        {
            Default,
            EditMode,
            NewObject,
            NewRule,
            RuleComposition
        }
        private UIState _uiState;
        public UIState UIstate
        {
            get => _uiState;
            set => _uiState = value;
        }
        public UIState State
        {
            get => _uiState;
            set { _uiState = value; }
        }
        private TextMeshProUGUI text;
        public TextMeshProUGUI Text
        {
            get => text;
            set { text = value; }
        }
        
        public InteractionCreationController InteractionCreationController
        {
            get => _interactionCreationController;
            set => _interactionCreationController = value;
        }
        
        private void Start()
        {
            _uiState = UIState.Default;
            textGo = GameObject.FindGameObjectWithTag("debugText");
            text = textGo.GetComponent<TextMeshProUGUI>();
            _editModeController = eventHandler.GetComponent<EditModeController>();
            _objectsMenuController = eventHandler.GetComponent<ObjectsMenuController>();
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
            DefaultState();
            /*radialMenu.getListButtons(optionButtons);
            radialMenu.gameObject.SetActive(true);*/
        }

        public void DeActivatePreviousState()
        {
            var previousState = _uiState;
            
            switch (previousState)
            {
                case UIState.EditMode:
                    _editModeController.DeActivateEditMode();
                    break;
                case UIState.NewObject:
                    _objectsMenuController.DeActivateObjectsMenu();
                    break;
                case UIState.NewRule:
                    _interactionCreationController.DeActivateNewRule();
                    break;
                case UIState.RuleComposition:
                    _interactionCreationController.DeActivateRuleComposition();
                    break;
            }
        }

        public void Close()
        {
            DeActivatePreviousState();
            DefaultState();

            if (isRecording)
                _interactionCreationController.StopRecording();
            
            ShowOptionsMenu();
            
            //closeButton.SetActive(false);
            /*radialMenu.RemoveSingleButtonToList(closeButton);*/
        }

        public void DefaultState()
        {
            text.text = "Choose if you want to create an object, modify an existing one or create a rule";
            _uiState = UIState.Default;
        }
        
        public void EditModeState()
        {
            DeActivatePreviousState();
            _uiState = UIState.EditMode;
            text.text = "You can modify the scene properties or select an object to modify";    
            HideOptionsMenu();
            /*radialMenu.getListButtons(new List<GameObject>(){editSceneButton});*/

        }
        

        public void NewRuleState()
        {
            if(_uiState != UIState.NewRule) DeActivatePreviousState();
            _uiState = UIState.NewRule;
            text.text = "Please, grab the modality you want to use to create the rule";
            HideOptionsMenu();
            /*radialMenu.getListButtons(ruleButtons);*/
        }

        public void CombineRulesState()
        {
            DeActivatePreviousState();
            _uiState = UIState.RuleComposition;
            HideDebugPanel();
            HideRadialMenu();
            
        }

        public void SelectedObject(string name)
        {
            text.text = "Selected object " + name;
        }

        public void SetDebugText(string text)
        {
            this.text.text = text;
        }
        
        public void NewObjectState()
        {
            DeActivatePreviousState();
            _uiState = UIState.NewObject;
            text.text = "Please, select the object you want to create";
            HideOptionsMenu();
            /*radialMenu.getListButtons(creationButtons);*/
        }

        private void HideOptionsMenu()
        {
            foreach (var button in optionButtons)
            {
                button.SetActive(false);
            }
        }
        
        public void ShowOptionsMenu()
        {
            foreach (var button in optionButtons)
            {
                button.SetActive(true);
            }
        }
        
        private void HideCreationMenu()
        {
            foreach (var button in creationButtons)
            {
                button.SetActive(false);
            }
        }

        private void HideRuleMenu()
        {
            foreach (var button in ruleButtons)
            {
                button.SetActive(false);
            }
        }

        private void HideEditMenu()
        {
            foreach (var button in editingButtons)
            {
                button.SetActive(false);
            }

            foreach (var button in characterButtons)
            {
                button.SetActive(false);
            }
            
            foreach (var button in musicButtons)
            {
                button.SetActive(false);
            }
            
            foreach (var button in lightButtons)
            {
                button.SetActive(false);
            }
            
            foreach (var button in effectButtons)
            {
                button.SetActive(false);
            }
            
            foreach (var button in stateDependentButtons)
            {
                button.SetActive(false);
            }
            
            foreach (var button in editSceneButtons)
            {
                button.SetActive(false);
            }
            
            foreach (var button in doorButtons)
            {
                button.SetActive(false);
            }
            
            closeButton.SetActive(false);
            editSceneButton.SetActive(false);
            
        }

        public void resetEditButtons()
        {
            editingButtonsTMP.Clear();
            editingButtonsTMP.AddRange(editingButtons);
        }
        
        public void DefaultRM()
        {
            HideOptionsMenu();
            HideCreationMenu();
            HideEditMenu();
            HideRuleMenu();
            /*radialMenu.getListButtons(optionButtons);*/
            resetEditButtons();
            ShowDebugPanel();
        }

        

        public void HideDebugPanel()
        {
            debugWindow.SetActive(false);
        }
        
        public void ShowDebugPanel()
        {
            debugWindow.SetActive(true);
        }

        public void HideRadialMenu()
        {
            /*radialMenu.gameObject.SetActive(false);*/
        }

        /*public List<GameObject> GetActiveButtons()
        {
            /*return radialMenu.PressableButtons;#1#
        }*/

        public void AddCombineRulesButtonToRadialMenu()
        {
            GameObject combineButton = optionButtons[optionButtons.Count - 1];
            /*if(combineButton.name.Equals("CombineRules"))
                radialMenu.AddSingleButtonToList(combineButton);
            else Debug.LogError("Combine button not found");*/
        }

        public void SwitchButtonTo (string name)
        {
            switch (name)
            {
                case "Show":
                    ChangeButton("Hide","Show");
                    break;
                case "Hide":
                    ChangeButton("Show", "Hide");
                    break;
                case "Stop":
                    ChangeButton("Play", "Stop");
                    break;
                case "Play":
                    ChangeButton("Stop", "Play");
                    break;
                case "GravityOFF":
                    ChangeButton("GravityON","GravityOFF");
                    break;
                case "GravityON":
                    ChangeButton("GravityOFF", "GravityON");
                    break;
                case "TurnOn":
                    ChangeButton("TurnOff","TurnOn");
                    break;
                case "TurnOff":
                    ChangeButton("TurnOn","TurnOff");
                    break;
                case "OpenDoor":
                    ChangeButton("CloseDoor","OpenDoor");
                    break;
                case "CloseDoor":
                    ChangeButton("OpenDoor","CloseDoor");
                    break;
            }
        }
        public void ChangeButton(string prevButtonName, string newButtonName)
        {
            GameObject newButton = null, prevButton = null;
            foreach (var button in stateDependentButtons)
            {
                if (button.name.Equals(newButtonName))
                {
                    newButton = button;
                    button.SetActive(true);
                }
                else if (button.name.Equals(prevButtonName))
                {
                    prevButton = button;
                    button.SetActive(false);
                }
                    
            }
            
            if(newButton==null || prevButton==null) return;
            /*int indexButtonToBeRemoved = radialMenu.PressableButtons.IndexOf(prevButton);
            //Substitute with the new one
            radialMenu.PressableButtons[indexButtonToBeRemoved] = newButton;
            radialMenu.getListButtons(radialMenu.PressableButtons, isRecording);*/
        }

        public void ShowEditSceneMenu()
        {
            /*radialMenu.getListButtons(editSceneButtons);*/
            text.text = "You can modify the scene properties"; 
        }

    }
}