using System;
using System.Collections.Generic;
using TMPro;
using UI.RuleEditor;
using UnityEngine;

namespace UI
{
    public class GeneralUIController: MonoBehaviour
    {
        private GameObject textGo;
        public GameObject debugWindow;
        public List<GameObject> stateDependentButtons;
        /*public GameObject closeButton;*/
        public GameObject eventHandler;
        private EditModeController _editModeController;
        private ObjectsMenuController _objectsMenuController;
        private InteractionCreationController _interactionCreationController;
        public HandMenuManager handMenuManager;
        private Prototypation _prototypation;
        public bool isRecording = false;
        private GameObject _selectedObject;
        
        // Test poop just to make it work
        public Boolean test = false;
        

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
            handMenuManager.ShowMenu(_uiState);
        }
        
        public void NewObjectState()
        {
            _uiState = UIState.NewObject;
            text.text = "Please, select the object you want to create";
            HideOptionsMenu();
            handMenuManager.ShowMenu(_uiState);
        }
        
        public void EditModeState()
        {
            _uiState = UIState.EditMode;
            text.text = "You can select an object to modify";
            handMenuManager.ShowMenu(_uiState);

        }
        
        public void NewRuleState()
        {
            _uiState = UIState.NewRule;
            text.text = "Please, grab the modality you want to use to create the rule"; 
            handMenuManager.ShowMenu(_uiState);
        }

        public void CombineRulesState()
        {
            _uiState = UIState.RuleComposition;
            handMenuManager.ShowMenu(_uiState);
        }

        public void SetSelectedObject(GameObject _selectedObject)
        {
            this._selectedObject = _selectedObject;
            text.text = "Selected object " + _selectedObject.name;
        }
        
        public GameObject GetSelectedObject()
        {
            return _selectedObject;
        }

        public void SetDebugText(string text)
        {
            this.text.text = text;
        }


        private void HideOptionsMenu()
        {
            /*
            foreach (var button in optionButtons)
            {
                button.SetActive(false);
            }
            */
        }
        
        public void ShowOptionsMenu()
        {
            /*
            foreach (var button in optionButtons)
            {
                button.SetActive(true);
            }
            */
        }
        
        private void HideCreationMenu()
        {
            /*
            foreach (var button in creationButtons)
            {
                button.SetActive(false);
            }
            */
        }

        private void HideRuleMenu()
        {
            /*
            foreach (var button in ruleButtons)
            {
                button.SetActive(false);
            }*/
        }

        private void HideEditMenu()
        {
            /*
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
            */
        }

        public void resetEditButtons()
        {
            /*
            editingButtonsTMP.Clear();
            editingButtonsTMP.AddRange(editingButtons);
            */
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
            /*GameObject combineButton = optionButtons[optionButtons.Count - 1];
            if(combineButton.name.Equals("CombineRules"))
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
            /*text.text = "You can modify the scene properties";*/
        }

    }
}