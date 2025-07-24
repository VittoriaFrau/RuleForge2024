using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using ECAPrototyping.RuleEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.Subsystems;
using TMPro;
using UI.RuleEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI
{
    public class CombineRulesController : MonoBehaviour
    {
        //Rule composition
        private GameObject removableBarrier, cubePlate, whenSequentialRow, whenEquivalenceRow, thenSequentialRow;
        private TextMeshProUGUI whenText, thenText;
        [FormerlySerializedAs("ruleEditorPlate")] public GameObject ruleEditorPlatePrefab;
        public GameObject activeRulePlate;
        public GameObject modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant;
        private List<MeanwhileRule> activeMeanwhileRules = new List<MeanwhileRule>();
        private ECAEvent[] currentThenEvents;
        public EventSequenceTracker eventSequenceTracker;
        private Dictionary<GameObject, InteractionCreationController.Modalities> gameObjectsWithBindings = new ();
        private Dictionary<GameObject, Coroutine> pointingCoroutines = new();
        private bool isLaunching = false;


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
        private GameObject ruleDebugText, cubeHelp;
        public GameObject interactables;
        private InteractionCreationController interactionCreationController;


        private void Start()
        {
            interactionCreationController = GetComponent<InteractionCreationController>();
        }
        
        private void Update()
        {
            foreach (var rule in activeMeanwhileRules)
            {
                if (rule != null)
                {
                    rule.UpdateTimer(Time.deltaTime);
                }
            }
        }

        public void ActivateCombineRules(bool startFromScratch = true)
        {

            if (GeneralUIController.Instance.recordedEvents.Count == 0)
            {
                GeneralUIController.Instance.SetDebugText(
                    "No recorded actions, please use the record button to record actions");
                GeneralUIController.Instance._handMenuManager.menuContentCanvas.SetActive(true);
                GeneralUIController.Instance._handMenuManager.debugPanel.SetActive(true);
                GeneralUIController.Instance._handMenuManager.mainMenu.SetActive(true);
                GeneralUIController.Instance.UIstate = GeneralUIController.UIState.Default;
                return;
            }

            //Set the rule plate visible
            if (activeRulePlate == null || startFromScratch)
            {
                activeRulePlate = Instantiate(ruleEditorPlatePrefab, ruleEditorPlatePrefab.transform.parent);
                activeRulePlate.SetActive(true);
                CacheReferencesCurrentRulePlate();
            }
            else // there is already a rule plate
            {
                activeRulePlate.SetActive(true);
                if (!startFromScratch)
                {
                    List<ECAEvent> recordedEventsNotInRulePlate = GeneralUIController.Instance.recordedEvents
                        .Where(x => x.CubeID == 0).ToList();
                    Debug.Log($"Recorded events not in rule plate: {recordedEventsNotInRulePlate.Count}");
                    Utils.GenerateCubesFromEventList(recordedEventsNotInRulePlate,
                        modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant, cubePlate);
                    removableBarrier.SetActive(false);
                    return;
                }
            }
            
            // Position the rule plate using the view of the main camera and add a small offset for the upper view
            activeRulePlate.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 3.0f;
            activeRulePlate.transform.localPosition = new Vector3(activeRulePlate.transform.localPosition.x, -1036f,
                activeRulePlate.transform.localPosition.z);

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
            activeRulePlate.SetActive(false);

            //Barrier to prevent the cubes from falling
            removableBarrier.SetActive(false);

            GeneralUIController.Instance.UIstate = GeneralUIController.UIState.Default;
            //GeneralUIController.Instance.DefaultState();
        }

        public void ResetCubePositions()
        {
            //Repositioning the plate in case the user has moved it
            activeRulePlate.transform.localPosition = new Vector3(-14.4f, -119.0f, 774.0f);

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
            actionCubesTuple = actionCubesTuple.OrderByDescending(x => x.Item1).ToList();

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
                .ToList().Find(x => x.name == "WhenText" && x.activeSelf).GetComponent<TextMeshProUGUI>();
            thenText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x => x.name == "ThenText" && x.activeSelf).GetComponent<TextMeshProUGUI>();

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
            if (cubeHelp) cubeHelp.SetActive(true);
            if(ruleDebugText) ruleDebugText.SetActive(false);
        }


        public void CalculateRuleOLD()
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

            eventSequenceTracker = new EventSequenceTracker(whenEvents, thenEvents, ruleEngine);

            // Bind dei whenEvents normali
            foreach (var whenEvent in whenEvents)
            {
                GameObject whenGameObject = whenEvent.ObjectRef;
                BindEvent(whenGameObject, whenEvent, eventSequenceTracker, false);
            }
            
            if (meanwhileEvents.Length > 0 && GeneralUIController.Instance.activeMeanwhileRules.Count != 0)
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
                        //DEMO
                        if (meanwhileEvent.Modality != InteractionCreationController.Modalities.Speech)
                        {
                            GameObject eventGameObject = meanwhileEvent.ObjectRef;
                            eventSequenceTracker = new EventSequenceTracker(new []{meanwhileEvent}, thenEvents, ruleEngine);
                            BindEvent(eventGameObject, meanwhileEvent, eventSequenceTracker, false, meanwhileRule);
                        }
                    }
                }
            }


            // Bind dell'equivalenceEvent (ne gestiamo uno solo per ora)
            if (equivalenceEvents.Length > 0)
            {
                ECAEvent equivalenceEvent = equivalenceEvents[0];
                GameObject equivalenceGameObject = equivalenceEvent.ObjectRef;
                BindEvent(equivalenceGameObject, equivalenceEvent, eventSequenceTracker, true);
            }
        }
        
        public void CalculateRule()
        {
            Debug.Log("Starting CalculateRule...");

            RuleEngine ruleEngine = RuleEngine.GetInstance();

            ECAEvent[] whenEvents = GetEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
            ECAEvent[] equivalenceEvents = GetEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });
            ECAEvent[] thenEvents = GetEventsFromContainers(thenSequentialRow, new[] { "ActionCubeContainer", "ActionCubeContainer(Clone)" });

            MeanwhileRule[] meanwhileEventsSequential = GetMeanwhileRulesFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
            MeanwhileRule[] meanwhileEventsEquivalence = GetMeanwhileRulesFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });

            currentThenEvents = thenEvents;

            Debug.Log($"Found {whenEvents.Length} 'when' events, {equivalenceEvents.Length} 'equivalence' events, and {thenEvents.Length} 'then' events.");
            Debug.Log($"Found {meanwhileEventsSequential.Length} sequential meanwhile rules and {meanwhileEventsEquivalence.Length} equivalence meanwhile rules.");

            if (whenEvents.Length == 0 && meanwhileEventsSequential.Length == 0)
            {
                Debug.LogWarning("No 'when' events or sequential meanwhile rules found. Aborting rule calculation.");
                return;
            }

            eventSequenceTracker = new EventSequenceTracker(whenEvents, thenEvents, ruleEngine);

            foreach (var whenEvent in whenEvents)
            {
                GameObject whenGameObject = whenEvent.ObjectRef;
                Debug.Log($"Binding 'when' event: {whenEvent} on GameObject: {(whenGameObject != null ? whenGameObject.name : "null")}");
                BindEvent(whenGameObject, whenEvent, eventSequenceTracker, false);
            }

            if ((meanwhileEventsSequential.Length > 0 || meanwhileEventsEquivalence.Length > 0) &&
                GeneralUIController.Instance.activeMeanwhileRules.Count != 0)
            {
                foreach (var meanwhileRule in meanwhileEventsSequential)
                {
                    if (!activeMeanwhileRules.Contains(meanwhileRule))
                    {
                        activeMeanwhileRules.Add(meanwhileRule);
                        Debug.Log($"Added sequential meanwhile rule: {meanwhileRule}");
                    }

                    foreach (var meanwhileEvent in meanwhileRule.events)
                    {
                        if (meanwhileEvent.Modality != InteractionCreationController.Modalities.Speech)
                        {
                            GameObject eventGameObject = meanwhileEvent.ObjectRef;
                            Debug.Log($"Binding sequential meanwhile event: {meanwhileEvent} on GameObject: {eventGameObject.name}");
                            eventSequenceTracker = new EventSequenceTracker(new[] { meanwhileEvent }, thenEvents, ruleEngine);
                            BindEvent(eventGameObject, meanwhileEvent, eventSequenceTracker, false, meanwhileRule);
                        }
                    }
                }

                foreach (var meanwhileRule in meanwhileEventsEquivalence)
                {
                    if (!activeMeanwhileRules.Contains(meanwhileRule))
                    {
                        activeMeanwhileRules.Add(meanwhileRule);
                        Debug.Log($"Added equivalence meanwhile rule: {meanwhileRule}");
                    }

                    foreach (var meanwhileEvent in meanwhileRule.events)
                    {
                        if (meanwhileEvent.Modality != InteractionCreationController.Modalities.Speech)
                        {
                            GameObject eventGameObject = meanwhileEvent.ObjectRef;
                            Debug.Log($"Binding equivalence meanwhile event: {meanwhileEvent} on GameObject: {eventGameObject.name}");
                            eventSequenceTracker = new EventSequenceTracker(new[] { meanwhileEvent }, thenEvents, ruleEngine);
                            BindEvent(eventGameObject, meanwhileEvent, eventSequenceTracker, false, meanwhileRule);
                        }
                    }
                }
            }

            if (equivalenceEvents.Length > 0)
            {
                ECAEvent equivalenceEvent = equivalenceEvents[0];
                GameObject equivalenceGameObject = equivalenceEvent.ObjectRef;
                Debug.Log($"Binding equivalence event: {equivalenceEvent} on GameObject: {equivalenceGameObject.name}");
                BindEvent(equivalenceGameObject, equivalenceEvent, eventSequenceTracker, true);
            }

            Debug.Log("Finished CalculateRule.");
        }



        private MeanwhileRule[] GetMeanwhileRulesFromContainers(GameObject row, string[] containerNames)
        {
            var allContainers = GetAllCubeContainers(row, containerNames);

            return allContainers
                .Select(container =>
                    Utils.GetMeanwhileRuleFromCube(container.currentCube, GeneralUIController.Instance.activeMeanwhileRules))
                .Where(rule => rule != null)
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
                var foundTransforms = Utils.FindAllChildrenWithName(row.transform, containerName);

                if (foundTransforms.Count == 0)
                {
                    Debug.LogWarning($"No transforms named '{containerName}' found under '{row.name}'");
                    continue;
                }

                foreach (var foundTransform in foundTransforms)
                {
                    var containers = foundTransform
                        .GetComponentsInChildren<CubeContainer>()
                        .Where(c => c.currentCube != null);

                    allContainers.AddRange(containers);
                }
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

        private void CacheReferencesCurrentRulePlate()
        {
            // Cache references to the current rule plate components
            removableBarrier = activeRulePlate.transform.Find("CubePlate/Barriers").gameObject;
            whenText = activeRulePlate.transform.Find("RuleText/Action Button/Frontplate/AnimatedContent/WhenTextContainer/WhenText").GetComponent<TextMeshProUGUI>();
            thenText = activeRulePlate.transform.Find("RuleText/Action Button/Frontplate/AnimatedContent/ThenTextContainer/ThenText").GetComponent<TextMeshProUGUI>();
            whenSequentialRow = activeRulePlate.transform.Find("RulePlate/When/Frontplate/SequentialRow").gameObject;
            whenEquivalenceRow = activeRulePlate.transform.Find("RulePlate/When/Frontplate/EquivalenceRow").gameObject;
            thenSequentialRow = activeRulePlate.transform.Find("RulePlate/Then/Frontplate/SequentialRow").gameObject;
            cubePlate = activeRulePlate.transform.Find("CubePlate").gameObject;
            cubeHelp = activeRulePlate.transform.Find("HelpText/Action Button/Frontplate/CubeHelp").gameObject;
            ruleDebugText = activeRulePlate.transform.Find("HelpText/Action Button/Frontplate/RuleDebugText").gameObject;
        }
        
        private void BindEvent(GameObject target, ECAEvent eventToBind, EventSequenceTracker tracker,
            bool isEquivalence, MeanwhileRule meanwhileRule = null)
        {
            Action<ECAEvent> triggerAction;
            
            //DEMO
            if (meanwhileRule != null)
            {
                ECAEvent laserEvent =
                    meanwhileRule.events.FirstOrDefault(e =>
                        e.Modality == InteractionCreationController.Modalities.Laser);
                if (laserEvent != null &&
                    laserEvent.EventCategory == CategoryController.CategoryObjectSelected.Category)
                {
                    triggerAction = (evt) => tracker.EventTriggered(evt);
                    var lastEcaScriptCategoryOfTarget = Utils.GetECALastScriptFromECAObject(laserEvent.ObjectRef);

                    foreach (var interactable in interactables.transform.GetComponentsInChildren<ObjectManipulator>())
                    {
                        if (Utils.GetECALastScriptFromECAObject(interactable.gameObject)
                            .Equals(lastEcaScriptCategoryOfTarget))
                        {
                            BindModalityEvent(interactable.gameObject, laserEvent, triggerAction);
                        }
                    }

                    return;
                }
            }

            if (meanwhileRule != null && !meanwhileRule.events.Any(e => e.Modality == InteractionCreationController.Modalities.Speech))
            {
                triggerAction = (evt) => OnMeanwhileEventTriggered(evt, meanwhileRule);
            }
            else
            {
                triggerAction = isEquivalence
                    ? (evt) => tracker.TriggerActionsDirectly(evt)
                    : (evt) => tracker.EventTriggered(evt);
            }

            if (eventToBind.EventCategory == CategoryController.CategoryObjectSelected.SingleObject)
            {
                BindModalityEvent(target, eventToBind, triggerAction);
            }
            else if (eventToBind.EventCategory == CategoryController.CategoryObjectSelected.Category)
            {
                var lastEcaScriptCategoryOfTarget = Utils.GetECALastScriptFromECAObject(target);

                foreach (var interactable in interactables.transform.GetComponentsInChildren<ObjectManipulator>())
                {
                    if (Utils.GetECALastScriptFromECAObject(interactable.gameObject).Equals(lastEcaScriptCategoryOfTarget))
                    {
                        BindModalityEvent(interactable.gameObject, eventToBind, triggerAction);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Unknown event category: {eventToBind.EventCategory}");
            }
        }

        
        private void BindModalityEvent(GameObject target, ECAEvent eventToBind, Action<ECAEvent> triggerAction)
        {
            if(target!=null)  gameObjectsWithBindings.Add(target, eventToBind.Modality); // Store the modality for unbinding later
            
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

                case InteractionCreationController.Modalities.Controller:
                    BindControllerEvent(eventToBind, triggerAction);
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
            UnityAction touchAction = () => triggerAction(ecaEvent);
            


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
            //DEMO
            //StartCoroutine()
        }

        private void BindControllerEvent(ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            var triggerInput = interactionCreationController.GetTriggerActionReference();
            if (triggerInput == null || triggerInput.action == null)
            {
                Debug.LogError("Trigger InputActionReference is not assigned.");
                return;
            }
            triggerInput.action.performed += ctx => triggerAction(ecaEvent);
            triggerInput.action.Enable();
        }

        private void BindLaserEvent(GameObject target, ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            var manipulator = target.GetComponent<ObjectManipulator>();
            if (manipulator == null)
            {
                Debug.LogWarning($"ObjectManipulator not found on {target.name}");
                return;
            }
            
            if(manipulator.enabled == false)
            {
                manipulator.enabled = true; // Ensure the manipulator is enabled
            }

            string verb = ecaEvent.EventStr.ToLower();

            if (verb.Contains("points") || verb.Contains("is pointing"))
            {
                manipulator.hoverEntered.AddListener(interactor =>
                {
                    //DEMO
                    if (!isLaunching)
                    {
                        isLaunching = true;
                        StartCoroutine(WaitForSecondsAndTriggerPointing(3f, ecaEvent, triggerAction, target));
                    }
                });
            }
            else if (verb.Contains("stops pointing"))
            {
                
                manipulator.hoverExited.AddListener(interactor =>
                {
                    //DEMO
                    isLaunching = false; // Reset the launching state
                    triggerAction(ecaEvent);
                });
            }
        }
        //DEMO
        private IEnumerator WaitForSecondsAndTriggerPointing(float seconds, ECAEvent ecaEvent, Action<ECAEvent> triggerAction, GameObject target)
        {
            yield return new WaitForSeconds(seconds);
            //pointingCoroutines.Remove(target); // cleanup after completion
            Debug.Log($"[Laser] Triggered after wait on {target.name}");
            triggerAction(ecaEvent);
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
        
        
        public void UnbindAllEvents()
        {

            foreach (GameObject go in gameObjectsWithBindings.Keys)
            {
                ObjectManipulator manipulator = go.GetComponent<ObjectManipulator>();
                switch (gameObjectsWithBindings[go])
                {
                    case InteractionCreationController.Modalities.Touch:
                        if (manipulator != null)
                        {
                            manipulator.OnClicked.RemoveAllListeners();
                            manipulator.selectEntered.RemoveAllListeners();
                            manipulator.selectExited.RemoveAllListeners();
                        }
                        break;
                    case InteractionCreationController.Modalities.Laser:
                        if (manipulator != null)
                        {
                            manipulator.hoverEntered.RemoveAllListeners();
                            manipulator.hoverExited.RemoveAllListeners();
                        }
                        break;
                        case InteractionCreationController.Modalities.Headgaze:
                        var gazeInteractor = interactionCreationController.gazeInteractor.GetComponent<FuzzyGazeInteractor>();
                        if (gazeInteractor != null)
                        {
                            gazeInteractor.hoverEntered.RemoveAllListeners();
                            gazeInteractor.hoverExited.RemoveAllListeners();
                        }

                        break;
                            
                }
                
            }
            // controllers
            var triggerInput = interactionCreationController.GetTriggerActionReference();
            if (triggerInput != null && triggerInput.action != null)
            {
                triggerInput.action.Disable();
            }
            
            //TODO proximity unbind
        }
        
       

    }
}