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
        

        public enum UIState
        {
            Default,
            EditMode,
            NewObject,
            NewInteraction,
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
            textGo = GameObject.FindGameObjectWithTag("debugText");
            text = textGo.GetComponent<TextMeshProUGUI>();
            _editModeController = eventHandler.GetComponent<EditModeController>();
            _objectsMenuController = eventHandler.GetComponent<ObjectsMenuController>();
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
            DefaultState();
        }

        public void DeActivatePreviousState(UIState newState)
        {
            var previousState = _uiState;
            if(previousState == newState) return;
            
            switch (previousState)
            {
                case UIState.EditMode:
                    _editModeController.UpdateAndRemoveListeners();
                    break;
                case UIState.NewObject:
                    break;
                case UIState.NewInteraction:
                    _interactionCreationController.DeActivateNewRule();
                    break;
                case UIState.RuleComposition:
                    _interactionCreationController.DeActivateRuleComposition();
                    break;
            }
        }

        public void DefaultState()
        {
            text.text = "Choose if you want to create an object, modify an existing one or create a rule";
            DeActivatePreviousState(UIState.Default);
            _uiState = UIState.Default;
            handMenuManager.HandleHandMenu(_uiState);
        }
        
        public void NewObjectState()
        {
            DeActivatePreviousState(UIState.NewObject);
            _uiState = UIState.NewObject;
            text.text = "Please, select the object you want to create";
            handMenuManager.HandleHandMenu(_uiState);
        }
        
        public void EditModeState()
        {
            DeActivatePreviousState(UIState.EditMode);
            _uiState = UIState.EditMode;
            if(_selectedObject == null) text.text = "You can select an object to modify";
            else text.text = "You are modifying the object " + _selectedObject.name;
            _editModeController.UpdateAndAddListeners();
            handMenuManager.HandleHandMenu(_uiState);
        }
        
        public void NewInteractionState()
        {
            DeActivatePreviousState(UIState.NewInteraction);
            _uiState = UIState.NewInteraction;
            text.text = "Please, grab the modality you want to use to create the rule"; 
            handMenuManager.HandleHandMenu(_uiState);
            _interactionCreationController.ShowModalitiesBubbles();
        }

        public void CombineRulesState()
        {
            DeActivatePreviousState(UIState.RuleComposition);
            _uiState = UIState.RuleComposition;
            handMenuManager.HandleHandMenu(_uiState);
        }

        public void SetSelectedObject(GameObject _selectedObject)
        {
            this._selectedObject = _selectedObject;
            if(_selectedObject != null) 
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

    }
}