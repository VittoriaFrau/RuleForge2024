using System;
using System.Collections.Generic;
using RulePlate.Core;
using TMPro;
using UI.RuleEditor;
using UnityEngine;

namespace UI
{
    
    public class GeneralUIController: MonoBehaviour
    {
        public static GeneralUIController Instance;

        private GameObject textGo;
        public GameObject eventHandler;
        public bool isRecording = false;
        private GameObject _selectedObject;
        public List<ECAEvent> recordedEvents = new();
        public List<MeanwhileEvent> activeMeanwhileEvents = new ();
        public Dictionary<GameObject, Vector3> initialPositions = new ();
        public Material spawnMaterial;
        public GameObject existingRuleOptionsUI;
        private List<ECARule> activeRules = new();
        public List<ECARule> ActiveRules
        {
            get => activeRules;
            set => activeRules = value;
        }
        
        //All controllers TODO have one of each and always call them in all the files
        private EditModeController _editModeController;
        public EditModeController EditModeController
        {
            get => _editModeController;
            set => _editModeController = value;
        }

        private CombineRulesController _combineRulesController;
        public CombineRulesController CombineRulesController
        {
            get => _combineRulesController;
            set => _combineRulesController = value;
        }

        private InteractionCreationController _interactionCreationController;
        public InteractionCreationController InteractionCreationController
        {
            get => _interactionCreationController;
            set => _interactionCreationController = value;
        }

        private Prototypation _prototypation;
        public Prototypation Prototypation
        {
            get => _prototypation;
            set => _prototypation = value;
        }

        private ObjectsMenuController _objectsMenuController;
        public ObjectsMenuController ObjectsMenuController
        {
            get => _objectsMenuController;
            set => _objectsMenuController = value;
        }
        
        public HandMenuManager _handMenuManager;

        
        public enum UIState
        {
            Default,
            EditMode,
            NewObject,
            NewInteraction,
            RuleComposition,
            Play
        }
        private UIState _uiState;
        public UIState UIstate
        {
            get => _uiState;
            set => _uiState = value;
        }
        private TextMeshProUGUI text;
        
        
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        
        private void Start()
        {
            textGo = GameObject.FindGameObjectWithTag("debugText");
            text = textGo.GetComponent<TextMeshProUGUI>();
            _objectsMenuController = eventHandler.GetComponent<ObjectsMenuController>();
            _editModeController = eventHandler.GetComponent<EditModeController>();
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
            _combineRulesController = eventHandler.GetComponent<CombineRulesController>();
            if (_combineRulesController.activeRulePlate)
            {
                if(_combineRulesController.activeRulePlate.gameObject.activeSelf) _combineRulesController.ruleEditorPlatePrefab.gameObject.SetActive(false);
            }
            DefaultState();
        }
        
        public void ClearRecordedEvents()
        {
            recordedEvents.Clear();
        }

        public void DeActivatePreviousState(UIState newState)
        {
            var previousState = _uiState;
            if(previousState == newState) return;
            
            switch (previousState)
            {
                case UIState.EditMode:
                    _editModeController.UpdateAndRemoveListeners();
                    if (Instance.isRecording)
                    {
                        _interactionCreationController.StopRecording();
                    }
                    break;
                case UIState.NewObject:
                    break;
                case UIState.NewInteraction:
                    _interactionCreationController.DeActivateNewInteraction();
                    break;
                case UIState.RuleComposition:
                    _combineRulesController.DeActivateRuleComposition();
                    break;
                case UIState.Play:
                    _handMenuManager.DeActivatePlayStateMenu();
                    _combineRulesController.eventSequenceTracker.ExecuteOppositeActions();
                    _combineRulesController.UnbindAllEvents();
                    _interactionCreationController.ResetTimer();
                    break;
            }
        }

        public void DefaultState()
        {
            DeActivatePreviousState(UIState.Default);
            _uiState = UIState.Default;
            _handMenuManager.HandleHandMenu(_uiState);
            text.text = "Choose if you want to create an object, modify an existing one or create a rule";
        }
        
        public void NewObjectState()
        {
            DeActivatePreviousState(UIState.NewObject);
            _uiState = UIState.NewObject;
            text.text = "Please, select the object you want to create";
            _handMenuManager.HandleHandMenu(_uiState);
        }
        
        public void EditModeState()
        {
            DeActivatePreviousState(UIState.EditMode);
            _uiState = UIState.EditMode;
            if(_selectedObject == null) text.text = "You can select an object to modify";
            else
            {
                text.text = "You are modifying the object " + _selectedObject.name;
                RegisterInitialPosition(_selectedObject);
            }
            _editModeController.UpdateAndAddListeners();
            _handMenuManager.HandleHandMenu(_uiState);
        }
        
        public void NewInteractionState()
        {
            DeActivatePreviousState(UIState.NewInteraction);
            _uiState = UIState.NewInteraction;
            text.text = "Please, grab the modality you want to use to create the rule"; 
            _handMenuManager.HandleHandMenu(_uiState);
            _interactionCreationController.ShowModalitiesBubbles();
        }

        public void HandleRuleCombinationState()
        {
            if (_combineRulesController.activeRulePlate != null)
            {
                // A rule has already been created, we ask the user if they want to continue with the current rule or create a new one
                SetDebugText("A rule has already been created, do you want to continue with the current rule or create a new one?");
                existingRuleOptionsUI.SetActive(true);
                _handMenuManager.mainMenu.SetActive(false);
            }
            else CreateNewRule();
        }
        
        public void ContinueWithExistingRule()
        {
            DeActivatePreviousState(UIState.RuleComposition);
            _uiState = UIState.RuleComposition;
            _handMenuManager.HandleHandMenu(_uiState);
            existingRuleOptionsUI.SetActive(false);
            _combineRulesController.ActivateCombineRules(false);
            // we need to remove the last rule to make sure we are not duplicating it
            activeRules.RemoveAt(activeRules.Count - 1);
            _combineRulesController.UnbindAllEvents();
        }
        
        public void CreateNewRule()
        {
            DeActivatePreviousState(UIState.RuleComposition);
            _uiState = UIState.RuleComposition;
            _handMenuManager.HandleHandMenu(_uiState);
            existingRuleOptionsUI.SetActive(false);
            _combineRulesController.ActivateCombineRules(true);
        }
        
        public void PlayState()
        {
            DeActivatePreviousState(UIState.Play);
            _uiState = UIState.Play;
            _handMenuManager.HandleHandMenu(_uiState);
            _combineRulesController.CalculateRule();
            //if in the scene there is a duplicate object, destroy it   
            Utils.DestroySpawnedObjects(_interactionCreationController.interactablesParent.transform);
            Utils.ApplyOriginalMaterialToDuplicatedObjects(_interactionCreationController.interactablesParent.transform);
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
        
        //TODO remove and use the ones in ECAObject
        private void RegisterInitialPosition(GameObject obj)
        {
            if (!initialPositions.ContainsKey(obj))
            {
                initialPositions[obj] = obj.transform.position;
            }
        }

        public Vector3 GetInitialPosition(GameObject obj)
        {
            return initialPositions.TryGetValue(obj, out var pos) ? pos : obj.transform.position;
        }
        
    }
}