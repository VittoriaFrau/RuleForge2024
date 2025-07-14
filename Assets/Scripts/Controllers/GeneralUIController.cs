using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
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
        private EditModeController _editModeController;
        private InteractionCreationController _interactionCreationController;
        private CombineRulesController _combineRulesController;
        public HandMenuManager handMenuManager;
        private Prototypation _prototypation;
        public bool isRecording = false;
        private GameObject _selectedObject;
        public List<ECAEvent> recordedEvents = new();
        public List<MeanwhileRule> activeMeanwhileRules = new List<MeanwhileRule>();
        public Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
        public Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
        public Material spawnMaterial;
        
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
        
        public InteractionCreationController InteractionCreationController
        {
            get => _interactionCreationController;
            set => _interactionCreationController = value;
        }
        
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        
        private void Start()
        {
            textGo = GameObject.FindGameObjectWithTag("debugText");
            text = textGo.GetComponent<TextMeshProUGUI>();
            _editModeController = eventHandler.GetComponent<EditModeController>();
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
            _combineRulesController = eventHandler.GetComponent<CombineRulesController>();
            if(_combineRulesController.activeRulePlate.gameObject.activeSelf) _combineRulesController.ruleEditorPlatePrefab.gameObject.SetActive(false);
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
                    handMenuManager.DeActivatePlayStateMenu();
                    _combineRulesController.eventSequenceTracker.ExecuteOppositeActions();
                    break;
            }
        }

        public void DefaultState()
        {
            DeActivatePreviousState(UIState.Default);
            _uiState = UIState.Default;
            handMenuManager.HandleHandMenu(_uiState);
            text.text = "Choose if you want to create an object, modify an existing one or create a rule";
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
            else
            {
                text.text = "You are modifying the object " + _selectedObject.name;
                RegisterInitialPosition(_selectedObject);
                RegisterOriginalMaterial(_selectedObject);
            }
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
            _combineRulesController.ActivateCombineRules();
        }
        
        public void PlayState()
        {
            DeActivatePreviousState(UIState.Play);
            _uiState = UIState.Play;
            handMenuManager.HandleHandMenu(_uiState);
            _combineRulesController.CalculateRule();
            _combineRulesController.activeRulePlate = null;
            ClearRecordedEvents();
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
        
        public void RegisterOriginalMaterial(GameObject obj)
        {
            if (!originalMaterials.ContainsKey(obj))
            {
                Renderer renderer = obj.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    originalMaterials[obj] = renderer.material;
                }
            }
        }

        public Material GetOriginalMaterial(GameObject obj)
        {
            return originalMaterials.TryGetValue(obj, out var mat) ? mat : null;
        }

    }
}