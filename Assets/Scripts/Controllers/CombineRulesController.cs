using System;
using System.Collections.Generic;
using System.Linq;
using ECAPrototyping.RuleEngine;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UI.RuleEditor;
using UnityEngine;

namespace UI
{
    public class CombineRulesController : MonoBehaviour
    {
        //Rule composition
        public GameObject removableBarrier;
        public TextMeshProUGUI whenText, thenText;
        public GameObject ruleEditorPlate;
        public GameObject cubePlate, modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant;
        public GameObject whenSequentialRow, whenEquivalenceRow, thenSequentialRow;
        private List<MeanwhileRule> activeMeanwhileRules = new List<MeanwhileRule>();
        private ECAEvent[] currentThenEvents;


        public enum ContainerType
        {
            Equivalence,
            Sequential
        }

        public enum RulePhase
        {
            When,
            Then,
            None
        }

        private List<CubeContainerClass> whenContainers;
        private List<CubeContainerClass> thenContainers;
        public GameObject modalityContainerPrefab, actionContainerPrefab;
        public GameObject ruleDebugText, cubeHelp;
        public GameObject interactables;
        private HandMenuManager handMenuManager;
        private InteractionCreationController interactionCreationController;
        public GameObject MRTKSpeech;

        private void Start()
        {
            handMenuManager = GeneralUIController.Instance.handMenuManager;
            interactionCreationController = GetComponent<InteractionCreationController>();
        }
        
        private void Update()
        {
            foreach (var rule in activeMeanwhileRules)
            {
                rule.UpdateTimer(Time.deltaTime);
            }
        }



        public void ActivateCombineRules()
        {

            if (GeneralUIController.Instance.recordedEvents.Count == 0)
            {
                GeneralUIController.Instance.SetDebugText(
                    "No recorded actions, please use the record button to record actions");
                handMenuManager.menuContentCanvas.SetActive(true);
                handMenuManager.debugPanel.SetActive(true);
                handMenuManager.mainMenu.SetActive(true);
                return;
            }

            //Set the rule plate visible
            ruleEditorPlate.SetActive(true);

            //Barrier to prevent the cubes from falling
            removableBarrier.SetActive(true);

            Utils.GenerateCubesFromEventList(GeneralUIController.Instance.recordedEvents,
                modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant, cubePlate);

            removableBarrier.SetActive(false);

            InitializeVariables();
        }


        public void DeActivateRuleComposition()
        {
            //Set the rule plate visible
            ruleEditorPlate.SetActive(false);

            //Barrier to prevent the cubes from falling
            removableBarrier.SetActive(false);

            GeneralUIController.Instance.UIstate = GeneralUIController.UIState.Default;
            GeneralUIController.Instance.DefaultState();
        }

        public void ResetCubePositions()
        {
            //Repositioning the plate in case the user has moved it
            ruleEditorPlate.transform.localPosition = new Vector3(-14.4f, -119.0f, 774.0f);

            // Using the original position of the cube, position it again there
            foreach (var recordedEvent in GeneralUIController.Instance.recordedEvents)
            {
                GameObject cube = recordedEvent.ObjectRef;
                cube.transform.localPosition = recordedEvent.CubeInitialPosition;
            }

            Utils.ClearTextDescription(whenText, thenText);

            Utils.ResetCubeContainers();
        }



        public void AutomaticCubePosition()
        {
            //Modality events
            //Find all the gameobjects with tag RuleCubes and save to modalityCubes if the name contains "Modality"
            List<GameObject> modalityCubes = GameObject.FindGameObjectsWithTag("RuleCubes")
                .Where(obj => obj.name.Contains("Modality")).ToList();
            //Position of the first cube container
            Vector3 firstCubeContainerWhenLocalPosition = new Vector3(144f, -50f, -18f);
            //Lista con i gameobject e l'indice che ne determina l'ordine di creazione dei cubi
            List<Tuple<int, GameObject>> modalityCubesTuple = new();

            foreach (ECAEvent ecaEvent in GeneralUIController.Instance.recordedEvents)
            {
                if (!ecaEvent.IsActionEvent)
                {
                    foreach (GameObject cube in modalityCubes)
                    {
                        if (ecaEvent.CubeID == cube.GetComponent<CubeController>().cubeID)
                            modalityCubesTuple.Add(new Tuple<int, GameObject>(ecaEvent.CubeID, cube));
                    }
                }
            }

            //Scorro la lista di tuple e ordino i cubi in base all'indice
            modalityCubesTuple = modalityCubesTuple.OrderBy(x => x.Item1).ToList();

            float xOffsetBetweenCubes = 50f; // Distanza fissa tra i cubi lungo l'asse x


            //Posiziono il primo cubo in firstCubeContainerLocalPosition e i successivi in base alla posizione del precedente
            for (int i = 0; i < modalityCubesTuple.Count; i++)
            {
                GameObject cube = modalityCubesTuple[i].Item2;
                cube.transform.localPosition =
                    firstCubeContainerWhenLocalPosition + new Vector3(i * xOffsetBetweenCubes, 0, 0);
            }

            //Action events
            //Find all the gameobjects with tag RuleCubes and save to modalityCubes if the name contains "Action"
            List<GameObject> actionCubes = GameObject.FindGameObjectsWithTag("ActionRuleCube")
                .Where(obj => obj.name.Contains("Action")).ToList();
            //Position of the first cube container
            Vector3 firstCubeContainerThenLocalPosition = new Vector3(144f, -83f, -21f);
            //Lista con i gameobject e l'indice che ne determina l'ordine di creazione dei cubi
            List<Tuple<int, GameObject>> actionCubesTuple = new();


            foreach (ECAEvent action in GeneralUIController.Instance.recordedEvents)
            {
                if (action.IsActionEvent)
                {
                    foreach (GameObject cube in actionCubes)
                    {
                        if (action.CubeID == cube.GetComponent<CubeController>().cubeID)
                            actionCubesTuple.Add(new Tuple<int, GameObject>(action.CubeID, cube));
                    }
                }

            }

            //Scorro la lista di tuple e ordino i cubi in base all'indice
            actionCubesTuple = actionCubesTuple.OrderBy(x => x.Item1).ToList();

            //Posiziono il primo cubo in firstCubeContainerLocalPosition e i successivi in base alla posizione del precedente
            for (int i = 0; i < actionCubesTuple.Count; i++)
            {
                GameObject cube = actionCubesTuple[i].Item2;
                cube.transform.localPosition =
                    firstCubeContainerThenLocalPosition + new Vector3(i * xOffsetBetweenCubes, 0, 0);
            }
        }

        //I need a function since it will be called as soon as the rule mode is on
        public void InitializeVariables()
        {
            whenText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x => x.name == "WhenText").GetComponent<TextMeshProUGUI>();
            thenText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x => x.name == "ThenText").GetComponent<TextMeshProUGUI>();

            //Adds the default containers
            whenContainers = new List<CubeContainerClass>();
            GameObject firstWhenContainer = whenSequentialRow.transform.Find("CubeContainer").gameObject;
            AddContainer(RulePhase.When, firstWhenContainer);
            thenContainers = new List<CubeContainerClass>();
            GameObject firstThenContainer = thenSequentialRow.transform.Find("ActionCubeContainer").gameObject;
            AddContainer(RulePhase.Then, firstThenContainer);
        }

        public void AddContainer(RulePhase rulePhase, GameObject containerGo)
        {
            CubeContainer cubeContainer = containerGo.GetComponent<CubeContainer>();
            if (rulePhase == RulePhase.When)
            {
                whenContainers.Add(new CubeContainerClass(whenContainers.Count, cubeContainer));
                cubeContainer.id = whenContainers.Count;
            }
            else
            {
                thenContainers.Add(new CubeContainerClass(thenContainers.Count, cubeContainer));
                cubeContainer.id = thenContainers.Count;
            }

        }


        /**
         * cube: the cube that has been moved (in or out)
         */
        public void CalculateRuleText(GameObject cube, RulePhase rulePhase, bool isAdded, ContainerType containerType,
            int id)
        {
            UpdatePresentRule(); //Updates the textmeshpro variables with the current rule text
            string cubeDescription = Utils.GetRuleDescriptionFromCubePrefab(cube.gameObject);
            string formattedCubeDescription = cubeDescription.Replace("\n", " ");
            if (isAdded)
            {
                string logicalOperator = containerType == ContainerType.Equivalence ? "OR" : ",";
                Utils.GenerateTextFromCubePosition(rulePhase == RulePhase.When ? whenText : thenText,
                    formattedCubeDescription, logicalOperator);
            }
            else
            {
                string logicalOperator = containerType == ContainerType.Equivalence ? "OR" : ",";
                switch (rulePhase)
                {
                    case RulePhase.When:
                        CubeContainerClass whenContainer = FindContainerById(id, whenContainers);
                        //Look in the when text for the cube description and remove it
                        Utils.RemoveTextFromCubePosition(whenText, formattedCubeDescription, logicalOperator);
                        if (string.IsNullOrWhiteSpace(whenText.text) || whenText.text == logicalOperator)
                        {
                            whenText.text = "...";
                        }

                        break;
                    case RulePhase.Then:
                        CubeContainerClass thenContainer = FindContainerById(id, thenContainers);
                        Utils.RemoveTextFromCubePosition(thenText, formattedCubeDescription, logicalOperator);
                        if (string.IsNullOrWhiteSpace(thenText.text) || whenText.text == logicalOperator)
                        {
                            thenText.text = "...";
                        }

                        break;
                }
            }
        }

        private void UpdatePresentRule()
        {
            whenText = whenText.GetComponent<TextMeshProUGUI>();
            thenText = thenText.GetComponent<TextMeshProUGUI>();
        }

        public void RemoveContainer(RulePhase rulePhase)
        {
            if (rulePhase == RulePhase.When)
            {
                CubeContainerClass container = FindContainerById(whenContainers.Count, whenContainers);
                if (container != null) whenContainers.Remove(container);
            }
            else
            {
                CubeContainerClass container = FindContainerById(thenContainers.Count, thenContainers);
                if (container != null) thenContainers.Remove(container);
            }
        }

        private CubeContainerClass FindContainerById(int id, List<CubeContainerClass> list)
        {
            foreach (var cont in list)
            {
                if (cont.id == id) return cont;
            }

            return null;
        }

        public void ActivateDebugTextWithMessage(string message)
        {
            cubeHelp.SetActive(false);
            ruleDebugText.SetActive(true);
            ruleDebugText.GetComponent<TextMeshProUGUI>().text = message;
        }

        public void DeactivateRuleDebugText()
        {
            cubeHelp.SetActive(true);
            ruleDebugText.SetActive(false);
        }


        public void CalculateRule()
        {
            RuleEngine ruleEngine = RuleEngine.GetInstance();

            ECAEvent[] whenEvents =
                GetEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
            ECAEvent[] equivalenceEvents =
                GetEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });
            ECAEvent[] thenEvents = GetEventsFromContainers(thenSequentialRow,
                new[] { "ActionCubeContainer", "ActionCubeContainer(Clone)" });
            MeanwhileRule [] meanwhileEvents = GetMeanwhileRulesFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });

            currentThenEvents = thenEvents;

            if (whenEvents.Length == 0 && meanwhileEvents.Length == 0)
            {
                Debug.LogWarning("No 'when' events found!");
                return;
            }

            EventSequenceTracker tracker = new EventSequenceTracker(whenEvents, thenEvents, ruleEngine);

            // Bind dei whenEvents normali
            foreach (var whenEvent in whenEvents)
            {
                GameObject whenGameObject = whenEvent.ObjectRef;
                BindEvent(whenGameObject, whenEvent, tracker, false);
            }
            
            if (meanwhileEvents.Length > 0)
            {
                foreach (var meanwhileRule in meanwhileEvents)
                {
                    // Add the rule to activeMeanwhileRules 
                    if (!activeMeanwhileRules.Contains(meanwhileRule))
                    {
                        activeMeanwhileRules.Add(meanwhileRule);
                    }
                    foreach (var meanwhileEvent in meanwhileRule.events)
                    {
                        GameObject eventGameObject = meanwhileEvent.ObjectRef;
                        BindEvent(eventGameObject, meanwhileEvent, tracker, false, meanwhileRule);
                    }
                }
            }


            // Bind dell'equivalenceEvent (ne gestiamo uno solo per ora)
            if (equivalenceEvents.Length > 0)
            {
                ECAEvent equivalenceEvent = equivalenceEvents[0];
                GameObject equivalenceGameObject = equivalenceEvent.ObjectRef;
                BindEvent(equivalenceGameObject, equivalenceEvent, tracker, true);
            }
        }


        private MeanwhileRule[] GetMeanwhileRulesFromContainers(GameObject row, string[] containerNames)
        {
            var allContainers = GetAllCubeContainers(row, containerNames);

            return allContainers
            .Select(container =>
                Utils.GetMeanwhileRuleFromCube(container.currentCube, GeneralUIController.Instance.activeMeanwhileRules))
            .ToArray();
        }


        private ECAEvent[] GetEventsFromContainers(GameObject row, string[] containerNames)
        {
            var allContainers = GetAllCubeContainers(row, containerNames);
                new List<CubeContainer>();
                
            return allContainers
                .Select(container =>
                    Utils.GetEventFromCube(container.currentCube, GeneralUIController.Instance.recordedEvents))
                .Where(e => e != null)
                .ToArray();
        }
        
        private List<CubeContainer> GetAllCubeContainers(GameObject row, string[] containerNames)
        {
            var allContainers = new List<CubeContainer>();

            foreach (var containerName in containerNames)
            {
                var foundTransform = row.transform.Find(containerName);
                if (foundTransform == null)
                {
                    Debug.LogWarning($"Transform '{containerName}' not found under '{row.name}'");
                    continue;
                }

                var containers = foundTransform
                    .gameObject
                    .GetComponentsInChildren<CubeContainer>()
                    .Where(c => c.currentCube != null);

                allContainers.AddRange(containers);
            }

            return allContainers;
        }
        
        private void OnMeanwhileEventTriggered(ECAEvent triggeredEvent, MeanwhileRule rule)
        {
            if (rule.HasTriggered(triggeredEvent))
                return;

            rule.RegisterTrigger(triggeredEvent);
            Debug.Log($"Meanwhile event triggered: {triggeredEvent.EventStr}");

            if (rule.IsComplete)
            {
                Debug.Log("All Meanwhile events triggered within time window! Executing actions...");
                ExecuteMeanwhileAction();
                rule.Reset();
            }
            else if (!rule.TimerRunning)
            {
                rule.StartTimer();
                Debug.Log($"Started timer for MeanwhileRule: {rule.timer}s");
            }
        }

        
        private void ExecuteMeanwhileAction()
        {

            // Esegui gli eventi THEN collegati, o le azioni che hai previsto
            foreach (var thenEvent in currentThenEvents)
            {
                RuleEngine.GetInstance().ExecuteAction(thenEvent.Action);
            }
        }



        private void BindEvent(GameObject target, ECAEvent eventToBind, EventSequenceTracker tracker,
            bool isEquivalence, MeanwhileRule meanwhileRule = null)
        {
            Action<ECAEvent> triggerAction;
            
            if (meanwhileRule != null)
            {
                triggerAction = (evt) => OnMeanwhileEventTriggered(evt, meanwhileRule);
            }
            else
            {
                triggerAction = isEquivalence
                    ? (evt) => tracker.TriggerActionsDirectly(evt)
                    : (evt) => tracker.EventTriggered(evt);
            }

            switch (eventToBind.Modality)
            {
                case InteractionCreationController.Modalities.Touch:
                    BindTouchEvent(target, eventToBind, triggerAction);
                    break;

                case InteractionCreationController.Modalities.Speech:
                    BindSpeechEvent(eventToBind, triggerAction);
                    break;

                case InteractionCreationController.Modalities.Laser:
                    BindLaserEvent(target, eventToBind, triggerAction);
                    break;

                case InteractionCreationController.Modalities.Headgaze:
                    BindHeadGazeEvent(target, eventToBind, triggerAction);
                    break;

                case InteractionCreationController.Modalities.Proximity:
                    BindProximityEvent(target, eventToBind, triggerAction);
                    break;

                default:
                    Debug.LogWarning($"Unknown modality: {eventToBind.Modality}");
                    break;
            }
        }


        private void BindTouchEvent(GameObject target, ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            var manipulator = target.GetComponent<ObjectManipulator>();
            if (manipulator == null)
            {
                Debug.LogWarning($"ObjectManipulator not found on {target.name}");
                return;
            }

            string verb = ecaEvent.EventStr.ToLower();

            if (verb.Contains("clicks") || verb.Contains("is clicking"))
            {
                manipulator.OnClicked.AddListener(() => triggerAction(ecaEvent));
            }
            else if (verb.Contains("selects") || verb.Contains("is selecting"))
            {
                manipulator.selectEntered.AddListener(interactor => triggerAction(ecaEvent));
            }
            else if (verb.Contains("deselects") || verb.Contains("is deselecting"))
            {
                manipulator.selectExited.AddListener(interactor => triggerAction(ecaEvent));
            }
        }

        private void BindSpeechEvent(ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
#if !UNITY_EDITOR
    MRTKSpeech.SetActive(true);

    var keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();
    if (keywordRecognitionSubsystem == null)
    {
        Debug.LogWarning("No running KeywordRecognitionSubsystem found");
        return;
    }

    string keyword = ecaEvent.EventStr; // comando di attivazione
    keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)
        .AddListener(() => triggerAction(ecaEvent));
#else
            Debug.LogWarning("Speech modality requires an XR headset and cannot be tested in the Unity Editor.");
#endif
        }

        private void BindLaserEvent(GameObject target, ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            var manipulator = target.GetComponent<ObjectManipulator>();
            if (manipulator == null)
            {
                Debug.LogWarning($"ObjectManipulator not found on {target.name}");
                return;
            }

            string verb = ecaEvent.EventStr.ToLower();

            if (verb.Contains("points") || verb.Contains("is pointing"))
            {
                manipulator.hoverEntered.AddListener(interactor => triggerAction(ecaEvent));
            }
            else if (verb.Contains("stops pointing"))
            {
                manipulator.hoverExited.AddListener(interactor => triggerAction(ecaEvent));
            }
        }

        private void BindHeadGazeEvent(GameObject target, ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            interactionCreationController.InstantiateHeadGazePointer();

            var gazeInteractor = interactionCreationController.gazeInteractor.GetComponent<FuzzyGazeInteractor>();
            if (gazeInteractor == null)
            {
                Debug.LogWarning($"FuzzyGazeInteractor not found");
                return;
            }

            string verb = ecaEvent.EventStr.ToLower();

            if (verb.Contains("looks") || verb.Contains("is looking"))
            {
                gazeInteractor.hoverEntered.AddListener(eventArgs =>
                {
                    var hoveredObject = eventArgs.interactableObject.transform.gameObject;
                    if (hoveredObject == target)
                    {
                        triggerAction(ecaEvent);
                    }
                });
            }
            else if (verb.Contains("stops looking"))
            {
                gazeInteractor.hoverExited.AddListener(eventArgs =>
                {
                    var hoveredObject = eventArgs.interactableObject.transform.gameObject;
                    if (hoveredObject == target)
                    {
                        triggerAction(ecaEvent);
                    }
                });
            }
        }

        private void BindProximityEvent(GameObject target, ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            var collider = target.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning($"Collider not found on {target.name}. Proximity detection requires a Collider.");
                return;
            }

            // We don't need isTrigger true
            if (collider.isTrigger)
            {
                collider.isTrigger = false;
            }

            ProximityTriggerListener listener = target.GetComponent<ProximityTriggerListener>();
            if (listener == null)
            {
                listener = target.AddComponent<ProximityTriggerListener>();
            }

            listener.OnProximityEnter += (other) =>
            {
                Debug.Log($"Proximity detected with {other.name}, triggering action.");
                triggerAction(ecaEvent);
            };
        }
    }
}