using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Controllers;
using ECAPrototyping.RuleEngine;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.Subsystems;
using RulePlate.Core;
using TMPro;
using UI.RuleEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UI
{
    public class CombineRulesController : MonoBehaviour
    {
        //Rule composition
        private GameObject removableBarrier, cubePlate, whenSequentialRow, whenEquivalenceRow, ifSequentialRow, thenSequentialRow;
        private TextMeshProUGUI whenText, ifText, thenText;
        [FormerlySerializedAs("ruleEditorPlate")] public GameObject ruleEditorPlatePrefab;
        public GameObject activeRulePlate;
        public GameObject modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant;
        private List<MeanwhileEvent> activeMeanwhileEvents = new();
        private ECAEvent[] currentThenEvents;
        private ECAEvent[] currentIfEvents;
        public EventSequenceTracker eventSequenceTracker;
        private Dictionary<GameObject, HashSet<InteractionCreationController.Modalities>> gameObjectsWithBindings = new();

        public enum ContainerType
        {
            Equivalence,
            Sequential
        }

        public enum RulePhase
        {
            When,
            If,
            Then,
            None
        }

        private List<CubeContainerClass> whenContainers;
        private List<CubeContainerClass> ifContainers;
        private List<CubeContainerClass> thenContainers;
        public GameObject modalityContainerPrefab, actionContainerPrefab;
        private GameObject ruleDebugText, cubeHelp;
        public GameObject interactables;
        private Action<InputAction.CallbackContext> controllerHandler;
        [SerializeField]
        private float offsetY = 0.2f;// Offset for the rule plate position

        
        
        private void Update()
        {
            foreach (var rule in activeMeanwhileEvents)
            {
                if (rule != null)
                {
                    rule.UpdateTimer(Time.deltaTime);
                }
            }
        }

        public void ActivateCombineRules(bool startFromScratch = true)
        {
            var recordedEvents = GeneralUIController.Instance.recordedEvents;

            // If there are no recorded events, show a message and display the UI menu
            if (recordedEvents.Count == 0)
            {
                GeneralUIController.Instance.SetDebugText("No recorded actions, please use the record button to record actions");
        
                var menuManager = GeneralUIController.Instance._handMenuManager;
                menuManager.menuContentCanvas.SetActive(true);
                menuManager.debugPanel.SetActive(true);
                menuManager.mainMenu.SetActive(true);
                GeneralUIController.Instance.UIstate = GeneralUIController.UIState.Default;
                return;
            }

            // Determine which events to use:
            // - If creating a rule for the first time, use all recorded events
            // - If a rule plate already exists, and startingFromScratch is true,
            // use only events that are not already in the plate
            // - Otherwise, only generate cubes for events not already in the plate (CubeID == 0)
            List<ECAEvent> eventsToGenerate;
            if (activeRulePlate == null)
            {
                // Case 1: First time creating a rule — use all events
                eventsToGenerate = recordedEvents;
            }
            else
            {
                // Case 2 & 3: Only generate cubes for events not already in the plate
                eventsToGenerate = recordedEvents.Where(e => e.CubeID == 0).ToList();
            }

            // If starting from scratch, create a new ruleplate and set it as active
            if (startFromScratch)
            {
                // Instantiate a new rule plate and cache its references
                activeRulePlate = Instantiate(ruleEditorPlatePrefab, ruleEditorPlatePrefab.transform.parent);
                CacheReferencesCurrentRulePlate();
                activeRulePlate.SetActive(true);

                // Position the rule plate in front of the camera and enable the physical barrier
                PositionRulePlateInFrontOfUser();
                removableBarrier.SetActive(true);
            }
            else
            {
                // If not starting from scratch, a rule plate already exists
                activeRulePlate.SetActive(true);
            }

            Debug.Log($"Generating {eventsToGenerate.Count} cube(s) for rule plate");

            // Generate cubes in the UI from the selected list of events
            Utils.GenerateCubesFromEventList(
                eventsToGenerate,
                modalityRuleCubePrefab,
                actionRuleCubePrefab,
                actionRuleCubePrefabVariant,
                cubePlate);

            // Hide the physical barrier after generation
            removableBarrier.SetActive(false);

            // Initialize any rule-specific state variables if we are starting from scratch
            if (startFromScratch)
            {
                InitializeVariables();
            }
        }

        
        private void PositionRulePlateInFrontOfUser()
        {
            // optimize for unity editor
            activeRulePlate.transform.localPosition = new Vector3(
                147.0f,
                -1290f,
                6324.0f);
            activeRulePlate.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            
            // Get the forward direction and position of the main camera
            /*Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraPosition = Camera.main.transform.position;

            // Get the Y position from the CameraOffset GameObject (parent of the main camera)
            float cameraOffsetY = Camera.main.transform.parent.position.y;

            // Calculate the target position 3 units in front of the camera
            Vector3 targetPosition = cameraPosition + cameraForward * 3.0f;

            // Set the object's position slightly below the camera's height
            activeRulePlate.transform.position = new Vector3(
                targetPosition.x,
                cameraOffsetY - offsetY, // lower by 0.5 units from CameraOffset height
                targetPosition.z
            );

            // Rotate the object to face the camera
            activeRulePlate.transform.localRotation = Quaternion.Euler(0f, -180f, 0f);*/
            

            /* // Place the rule plate 3 units in front of the camera and slightly offset vertically
             Vector3 cameraForward = Camera.main.transform.forward;
             Vector3 cameraPosition = Camera.main.transform.position;

             activeRulePlate.transform.position = cameraPosition + cameraForward * 3.0f;
             activeRulePlate.transform.localPosition = new Vector3(
                 activeRulePlate.transform.localPosition.x,
                 -1290f,
                 activeRulePlate.transform.localPosition.z);
             activeRulePlate.transform.localRotation = Quaternion.Euler(0f, -180f, 0f);*/
            /*
            if (GeneralUIController.Instance._handMenuManager.isUsingOculusLink)
            {
                activeRulePlate.transform.localPosition = new Vector3(-1118f, -973f, 5015f);
                activeRulePlate.transform.localRotation = new Quaternion(0f,267.89386f,0f, activeRulePlate.transform.localRotation.w);
            }
            else
            {
                // Place the rule plate 3 units in front of the camera and slightly offset vertically
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraPosition = Camera.main.transform.position;

                activeRulePlate.transform.position = cameraPosition + cameraForward * 3.0f;
                activeRulePlate.transform.localPosition = new Vector3(
                    activeRulePlate.transform.localPosition.x,
                    -1036f,
                    activeRulePlate.transform.localPosition.z);
            }*/
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

            Utils.ClearTextDescription(whenText, ifText, thenText);

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
            ifText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x => x.name == "IfText" && x.activeSelf).GetComponent<TextMeshProUGUI>();
            thenText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x => x.name == "ThenText" && x.activeSelf).GetComponent<TextMeshProUGUI>();

            //Adds the default containers
            whenContainers = new List<CubeContainerClass>();
            GameObject firstWhenContainer = whenSequentialRow.transform.Find("CubeContainer").gameObject;
            AddContainer(RulePhase.When, firstWhenContainer);
            ifContainers = new List<CubeContainerClass>();
            GameObject firstIfContainer = ifSequentialRow.transform.Find("ActionCubeContainer").gameObject;
            AddContainer(RulePhase.If, firstIfContainer);
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
            else if(rulePhase== RulePhase.If)
            {
                ifContainers.Add(new CubeContainerClass(ifContainers.Count, cubeContainer));
                cubeContainer.id = ifContainers.Count;
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

        public void CalculateRuleText(GameObject cube, RulePhase rulePhase, bool isAdded, ContainerType containerType, int id)
        {
            UpdatePresentRule(); //Updates the textmeshpro variables with the current rule text
            string cubeDescription = Utils.GetRuleDescriptionFromCubePrefab(cube.gameObject);
            string formattedCubeDescription = cubeDescription.Replace("\n", " ");
            //if we are adding text
            if (isAdded)
            {
                string logicalOperator = containerType == ContainerType.Equivalence ? "OR" : ",";
                if (rulePhase == RulePhase.When)
                    Utils.GenerateTextFromCubePosition(whenText, formattedCubeDescription, logicalOperator);
                else if (rulePhase == RulePhase.If)
                    Utils.GenerateTextFromCubePosition(ifText, formattedCubeDescription, logicalOperator);
                else
                    Utils.GenerateTextFromCubePosition(thenText, formattedCubeDescription, logicalOperator);

                if (rulePhase == RulePhase.Then)
                {
                    // Check if there is an any in the when phase
                    if (whenText.text.Contains("any"))
                    {
                        // we remove the numbers from the then text, so it's more generic (e.g. "cube" instead of "cube1")
                        thenText.text = Regex.Replace(thenText.text, @"\d+", "");
                    }
                }
            }
            // if we are removing text
            else
            {
                string logicalOperator = containerType == ContainerType.Equivalence ? "OR" : ",";
                switch (rulePhase)
                {
                    case RulePhase.When:
                        CubeContainerClass whenContainer = FindContainerById(id, whenContainers);
                        Utils.RemoveTextFromCubePosition(whenText, formattedCubeDescription, logicalOperator);
                        if (string.IsNullOrWhiteSpace(whenText.text) || whenText.text == logicalOperator)
                        {
                            whenText.text = "...";
                        }
                        break;
                    case RulePhase.If:
                        CubeContainerClass ifContainer = FindContainerById(id, ifContainers);
                        Utils.RemoveTextFromCubePosition(ifText, formattedCubeDescription, logicalOperator);
                        if (string.IsNullOrWhiteSpace(ifText.text) || ifText.text == logicalOperator)
                        {
                            ifText.text = "...";
                        }
                        break;
                    case RulePhase.Then:
                        CubeContainerClass thenContainer = FindContainerById(id, thenContainers);
                        Utils.RemoveTextFromCubePosition(thenText, formattedCubeDescription, logicalOperator);
                        if (string.IsNullOrWhiteSpace(thenText.text) || thenText.text == logicalOperator)
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
            ifText = ifText.GetComponent<TextMeshProUGUI>();
            thenText = thenText.GetComponent<TextMeshProUGUI>();
        }

        public void RemoveContainer(RulePhase rulePhase)
        {
            if (rulePhase == RulePhase.When)
            {
                CubeContainerClass container = FindContainerById(whenContainers.Count, whenContainers);
                if (container != null) whenContainers.Remove(container);
            }
            else if(rulePhase == RulePhase.If)
            {
                CubeContainerClass container = FindContainerById(ifContainers.Count, ifContainers);
                if (container != null) ifContainers.Remove(container);
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


        //public void CalculateRuleOLD()
        //{
        //    RuleEngine ruleEngine = RuleEngine.GetInstance();

        //    ECAEvent[] whenEvents =
        //        GetEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
        //    ECAEvent[] equivalenceEvents =
        //        GetEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });
        //    ECAEvent[] thenEvents = GetEventsFromContainers(thenSequentialRow,
        //        new[] { "ActionCubeContainer", "ActionCubeContainer(Clone)" });
        //    MeanwhileEvent [] meanwhileEvents = GetMeanwhileEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });

        //    currentThenEvents = thenEvents;

        //    if (whenEvents.Length == 0 && meanwhileEvents.Length == 0)
        //    {
        //        Debug.LogWarning("No 'when' events found!");
        //        return;
        //    }

        //    eventSequenceTracker = new EventSequenceTracker(whenEvents, thenEvents, ruleEngine);

        //    // Bind dei whenEvents normali
        //    foreach (var whenEvent in whenEvents)
        //    {
        //        GameObject whenGameObject = whenEvent.ObjectRef;
        //        BindEvent(whenGameObject, whenEvent, eventSequenceTracker, false);
        //    }
            
        //    if (meanwhileEvents.Length > 0 && GeneralUIController.Instance.activeMeanwhileEvents.Count != 0)
        //    {
        //        foreach (var meanwhileRule in meanwhileEvents)
        //        {
        //            // Add the rule to activeMeanwhileEvents 
        //            if (!activeMeanwhileEvents.Contains(meanwhileRule))
        //            {
        //                activeMeanwhileEvents.Add(meanwhileRule);
        //            }
        //            foreach (var meanwhileEvent in meanwhileRule.events)
        //            {
        //                //DEMO
        //                if (meanwhileEvent.Modality != InteractionCreationController.Modalities.Speech)
        //                {
        //                    GameObject eventGameObject = meanwhileEvent.ObjectRef;
        //                    eventSequenceTracker = new EventSequenceTracker(new []{meanwhileEvent}, thenEvents, ruleEngine);
        //                    BindEvent(eventGameObject, meanwhileEvent, eventSequenceTracker, false, meanwhileRule);
        //                }
        //            }
        //        }
        //    }


        //    // Bind dell'equivalenceEvent (ne gestiamo uno solo per ora)
        //    if (equivalenceEvents.Length > 0)
        //    {
        //        ECAEvent equivalenceEvent = equivalenceEvents[0];
        //        GameObject equivalenceGameObject = equivalenceEvent.ObjectRef;
        //        BindEvent(equivalenceGameObject, equivalenceEvent, eventSequenceTracker, true);
        //    }
        //}


        public void CalculateRule()
        {
            Debug.Log("Starting CalculateRule...");

            ECARule rule = BuildECARule();
            if(!GeneralUIController.Instance.ActiveRules.Contains(rule))
            {
                GeneralUIController.Instance.ActiveRules.Add(rule);
            }

            foreach (var r in GeneralUIController.Instance.ActiveRules)
            {
                if (r != null)
                {
                    Debug.Log(r.ToString());
                    BindECARule(r);
                }
            }

            Debug.Log("Finished CalculateRule.");
        }
        

        private ECARule BuildECARule()
        {
            ECAEvent[] whenEvents = GetEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
            ECAEvent[] equivalenceEvents = GetEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });
            ECAEvent[] ifEvents = GetEventsFromContainers(ifSequentialRow, new[] { "ActionCubeContainer", "ActionCubeContainer(Clone)" });
            ECAEvent[] thenEvents = GetEventsFromContainers(thenSequentialRow, new[] { "ActionCubeContainer", "ActionCubeContainer(Clone)" });

            MeanwhileEvent[] meanwhileEventsSequential = GetMeanwhileEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
            MeanwhileEvent[] meanwhileEventsEquivalence = GetMeanwhileEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });

            currentIfEvents = ifEvents;
            currentThenEvents = thenEvents;

            Debug.Log($"Found {whenEvents.Length} 'when' events, {equivalenceEvents.Length} 'equivalence' events, {ifEvents.Length} 'if' events, and {thenEvents.Length} 'then' events.");
            Debug.Log($"Found {meanwhileEventsSequential.Length} sequential meanwhile rules and {meanwhileEventsEquivalence.Length} equivalence meanwhile rules.");

            if (whenEvents.Length == 0 && meanwhileEventsSequential.Length == 0)
            {
                Debug.LogWarning("No 'when' events or sequential meanwhile rules found. Aborting rule calculation.");
                return null;
            }

            List<MeanwhileEvent> allMeanwhileEvents = new List<MeanwhileEvent>();
            allMeanwhileEvents.AddRange(meanwhileEventsSequential);
            allMeanwhileEvents.AddRange(meanwhileEventsEquivalence);

            ECARule rule;
            if (allMeanwhileEvents.Count > 0)
            {
                rule = new ECARule(new List<ECAEvent>(whenEvents), new List<ECAEvent>(thenEvents));
                rule.MeanwhileEvents = allMeanwhileEvents;
            }
            else
            {
                List<ECAEvent> allWhenEvents = new List<ECAEvent>(whenEvents);
                if (equivalenceEvents.Length > 0)
                {
                    equivalenceEvents[0].IsEquivalenceEvent = true;
                    allWhenEvents.Add(equivalenceEvents[0]);
                }

                rule = new ECARule(allWhenEvents, new List<ECAEvent>(thenEvents));
            }

            rule.IfEvents = new List<ECAEvent>(ifEvents);
            Debug.Log($"[CombineRules] Built rule: when={rule.Events?.Count ?? 0}, if={rule.IfEvents?.Count ?? 0}, then={rule.Actions?.Count ?? 0}");
            
            rule.MarkDynamicSubjects();

            return rule;
        }
        private void BindECARule(ECARule rule)
        {
            if (rule == null)
                return;

            RuleEngine ruleEngine = RuleEngine.GetInstance();
            var ifEvents = rule.IfEvents != null ? rule.IfEvents.ToArray() : Array.Empty<ECAEvent>();
            eventSequenceTracker = new EventSequenceTracker(rule.Events.ToArray(), ifEvents, rule.Actions.ToArray(), ruleEngine);
            Debug.Log($"[CombineRules] BindECARule with ifEvents={ifEvents.Length}, thenEvents={rule.Actions?.Count ?? 0}");

            // Bind normali ECAEvent (WHEN/EQUIVALENCE)
            if (rule.Events != null)
            {
                foreach (var ecaEvent in rule.Events)
                {
                    GameObject obj = ecaEvent.ObjectRef;
                    Debug.Log($"Binding 'when/equivalence' event: {ecaEvent} on GameObject: {(obj != null ? obj.name : "null")}");
                    var isEquivalence = ecaEvent.IsEquivalenceEvent;
                    BindEvent(obj, ecaEvent, eventSequenceTracker, isEquivalence); 
                }
            }

            // Bind MeanwhileEvent
            if (rule.MeanwhileEvents != null)
            {
                foreach (var meanwhileRule in rule.MeanwhileEvents)
                {
                    if (!activeMeanwhileEvents.Contains(meanwhileRule))
                    {
                        activeMeanwhileEvents.Add(meanwhileRule);
                        Debug.Log($"Added meanwhile rule: {meanwhileRule}");
                    }

                    foreach (var mEvent in meanwhileRule.events)
                    {
                        if (mEvent.Modality != InteractionCreationController.Modalities.Speech)
                        {
                            GameObject obj = mEvent.ObjectRef;
                            Debug.Log($"Binding meanwhile event: {mEvent} on GameObject: {obj.name}");
                            eventSequenceTracker = new EventSequenceTracker(new[] { mEvent }, ifEvents, rule.Actions.ToArray(), ruleEngine);
                            BindEvent(obj, mEvent, eventSequenceTracker, false, meanwhileRule);
                        }
                    }
                }
            }
        }

        /* public void CalculateRule()
         {
             Debug.Log("Starting CalculateRule...");

             RuleEngine ruleEngine = RuleEngine.GetInstance();

             ECAEvent[] whenEvents = GetEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
             ECAEvent[] equivalenceEvents = GetEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });
             ECAEvent[] thenEvents = GetEventsFromContainers(thenSequentialRow, new[] { "ActionCubeContainer", "ActionCubeContainer(Clone)" });

             MeanwhileEvent[] meanwhileEventsSequential = GetMeanwhileEventsFromContainers(whenSequentialRow, new[] { "CubeContainer", "CubeContainer(Clone)" });
             MeanwhileEvent[] meanwhileEventsEquivalence = GetMeanwhileEventsFromContainers(whenEquivalenceRow, new[] { "CubeContainer(Clone)" });

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
                 GeneralUIController.Instance.activeMeanwhileEvents.Count != 0)
             {
                 foreach (var meanwhileRule in meanwhileEventsSequential)
                 {
                     if (!activeMeanwhileEvents.Contains(meanwhileRule))
                     {
                         activeMeanwhileEvents.Add(meanwhileRule);
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
                     if (!activeMeanwhileEvents.Contains(meanwhileRule))
                     {
                         activeMeanwhileEvents.Add(meanwhileRule);
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
         }*/


        private MeanwhileEvent[] GetMeanwhileEventsFromContainers(GameObject row, string[] containerNames)
        {
            var allContainers = GetAllCubeContainers(row, containerNames);

            return allContainers
                .Select(container =>
                    Utils.GetMeanwhileEventFromCube(container.currentCube, GeneralUIController.Instance.activeMeanwhileEvents))
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

        
        private void OnMeanwhileEventTriggered(ECAEvent triggeredEvent, MeanwhileEvent @event)
        {
            if (@event.HasTriggered(triggeredEvent))
                return;

            @event.RegisterTrigger(triggeredEvent);
            Debug.Log($"Meanwhile event triggered: {triggeredEvent.EventStr}");

            if (@event.IsComplete)
            {
                Debug.Log("All Meanwhile events triggered within time window! Executing actions...");
                ExecuteMeanwhileAction();
                @event.Reset();
            }
            else if (!@event.TimerRunning)
            {
                @event.StartTimer();
                Debug.Log($"Started timer for MeanwhileEvent: {@event.timer}s");
            }
        }

        
        private void ExecuteMeanwhileAction()
        {
            if (!EventSequenceTracker.AreIfConditionsMet(currentIfEvents))
            {
                GeneralUIController.Instance.SetDebugText("IF condition not satisfied. THEN skipped.");
                Debug.Log("[CombineRules] IF condition not satisfied. THEN skipped.");
                return;
            }

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

            // Ensure the IF text UI exists (older prefabs only have WHEN + THEN).
            EnsureIfTextUIExists(activeRulePlate.transform);

            whenText = activeRulePlate.transform
                .Find("RuleText/Action Button/Frontplate/AnimatedContent/WhenTextContainer/WhenText")
                .GetComponent<TextMeshProUGUI>();
            ifText = activeRulePlate.transform
                .Find("RuleText/Action Button/Frontplate/AnimatedContent/IfTextContainer/IfText")
                .GetComponent<TextMeshProUGUI>();
            thenText = activeRulePlate.transform
                .Find("RuleText/Action Button/Frontplate/AnimatedContent/ThenTextContainer/ThenText")
                .GetComponent<TextMeshProUGUI>();

            whenSequentialRow = activeRulePlate.transform.Find("RulePlate/When/Frontplate/SequentialRow").gameObject;
            whenEquivalenceRow = activeRulePlate.transform.Find("RulePlate/When/Frontplate/EquivalenceRow").gameObject;

            // IF is optional in older prefabs; after EnsureIfTextUIExists it must exist.
            ifSequentialRow = activeRulePlate.transform.Find("RulePlate/If/Frontplate/SequentialRow").gameObject;

            thenSequentialRow = activeRulePlate.transform.Find("RulePlate/Then/Frontplate/SequentialRow").gameObject;
            cubePlate = activeRulePlate.transform.Find("CubePlate").gameObject;
            cubeHelp = activeRulePlate.transform.Find("HelpText/Action Button/Frontplate/CubeHelp").gameObject;
            ruleDebugText = activeRulePlate.transform.Find("HelpText/Action Button/Frontplate/RuleDebugText").gameObject;
        }

        /// <summary>
        /// Guarantees the presence of the IF section in the rule summary UI (RuleText panel).
        /// This keeps the UI consistent with ECA semantics: WHEN / IF / THEN.
        /// </summary>
        private static void EnsureIfTextUIExists(Transform rulePlateRoot)
        {
            var animatedContent = rulePlateRoot.Find("RuleText/Action Button/Frontplate/AnimatedContent");
            if (animatedContent == null)
            {
                Debug.LogWarning("AnimatedContent not found: cannot ensure IF text UI.");
                return;
            }

            // Already present
            if (animatedContent.Find("IfTextContainer") != null)
                return;

            var thenContainer = animatedContent.Find("ThenTextContainer");
            if (thenContainer == null)
            {
                Debug.LogWarning("ThenTextContainer not found: cannot clone for IF text UI.");
                return;
            }

            // Clone THEN container and adapt it to IF.
            var ifContainer = Instantiate(thenContainer.gameObject, animatedContent).transform;
            ifContainer.name = "IfTextContainer";
            ifContainer.SetSiblingIndex(1); // WHEN (0) -> IF (1) -> THEN (2)

            // Rename + retitle label
            var label = ifContainer.Find("ThenLabel");
            if (label != null)
            {
                label.name = "IfLabel";
                var labelTmp = label.GetComponent<TextMeshProUGUI>();
                if (labelTmp != null)
                {
                    labelTmp.text = labelTmp.text.Replace("THEN", "IF");
                }
            }

            // Rename + retitle text
            var thenText = ifContainer.Find("ThenText");
            if (thenText != null)
            {
                thenText.name = "IfText";
                thenText.tag = "RuleText";
                var tmp = thenText.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "...";
                }
            }
        }
        
        private void BindEvent(GameObject target, ECAEvent eventToBind, EventSequenceTracker tracker,
            bool isEquivalence, MeanwhileEvent meanwhileEvent = null)
        {
            Action<ECAEvent> triggerAction;
            
            //DEMO
            if (meanwhileEvent != null)
                return;
                
            //DEMO
            if (meanwhileEvent != null)
            {
                ECAEvent laserEvent =
                    meanwhileEvent.events.FirstOrDefault(e =>
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

            if (meanwhileEvent != null && !meanwhileEvent.events.Any(e => e.Modality == InteractionCreationController.Modalities.Speech))
            {
                triggerAction = (evt) => OnMeanwhileEventTriggered(evt, meanwhileEvent);
            }
            else
            {
                triggerAction = isEquivalence
                    ? (evt) => tracker.TriggerActionsDirectly(evt)
                    : (evt) => tracker.EventTriggered(evt);
            }

            if (eventToBind.EventCategory == CategoryController.CategoryObjectSelected.SingleObject)
            {
                //if the event is bound to a single object, we bind it directly to that object
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
            var modality = eventToBind.Modality;

            if (target == null)
            {
                // target can be null in case of controller or speech modality, where the event is not bound to a specific GameObject
                if (modality == InteractionCreationController.Modalities.Controller || modality == InteractionCreationController.Modalities.Speech)
                {
                    target = this.gameObject; // Use the eventHandler itself
                }
                else
                {
                    Debug.LogWarning("Target GameObject is null, cannot bind event.");
                    return;
                }
            }
            
            if (HasBinding(target, modality))
            {
                Debug.Log($"Modality {modality} already bound for {target.name}, skipping.");
                return;
            }

            // Store the modality for unbinding later
            AddBinding(target, modality);
            
            switch (modality)
            {
                case InteractionCreationController.Modalities.Touch:
                    BindTouchEvent(target, eventToBind, triggerAction);
                    break;

                //case InteractionCreationController.Modalities.Speech:
                //    BindSpeechEvent(eventToBind, triggerAction);
                //    break;

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
                manipulator.OnClicked.AddListener(() =>
                {
                    triggerAction(ecaEvent);
                });
            }
            else if (verb.Contains("selects") || verb.Contains("is selecting"))
            {
                manipulator.selectEntered.AddListener(interactor =>
                {
                    triggerAction(ecaEvent);
                });
            }
            else if (verb.Contains("deselects") || verb.Contains("is deselecting"))
            {
                manipulator.selectExited.AddListener(interactor => triggerAction(ecaEvent));
            }
        }

//        private void BindSpeechEvent(ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
//        {
//#if !UNITY_EDITOR
//    MRTKSpeech.SetActive(true);

//    var keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();
//    if (keywordRecognitionSubsystem == null)
//    {
//        Debug.LogWarning("No running KeywordRecognitionSubsystem found");
//        return;
//    }

//    string keyword = ecaEvent.EventStr; // comando di attivazione
//    keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)
//        .AddListener(() => triggerAction(ecaEvent));
//#else
//            Debug.LogWarning("Speech modality requires an XR headset and cannot be tested in the Unity Editor.");
//#endif
//            //DEMO
//            //StartCoroutine()
//        }

        private void BindControllerEvent(ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            var triggerInput = GeneralUIController.Instance.InteractionCreationController.GetTriggerActionReference();
            if (triggerInput == null || triggerInput.action == null)
            {
                Debug.LogError("Trigger InputActionReference is not assigned.");
                return;
            }
            controllerHandler = ctx => triggerAction(ecaEvent);
            
            triggerInput.action.performed += controllerHandler;

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
                    triggerAction(ecaEvent);
                });
            }
            else if (verb.Contains("stops pointing"))
            {
                manipulator.hoverExited.AddListener(interactor =>
                {
                    triggerAction(ecaEvent);
                });
            }
        }


        private void BindHeadGazeEvent(GameObject target, ECAEvent ecaEvent, Action<ECAEvent> triggerAction)
        {
            GeneralUIController.Instance.InteractionCreationController.InstantiateHeadGazePointer();

            var gazeInteractor = GeneralUIController.Instance.InteractionCreationController.gazeInteractor.GetComponent<FuzzyGazeInteractor>();
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
            foreach (var pair in gameObjectsWithBindings)
            {
                GameObject go = pair.Key;
                var modalities = pair.Value;

                if(go){
                    foreach (var modality in modalities){
                        var manipulator = go.GetComponent<ObjectManipulator>();
                        switch (modality)
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
                                var gazeInteractor = GeneralUIController.Instance.InteractionCreationController.gazeInteractor.GetComponent<FuzzyGazeInteractor>();
                                if (gazeInteractor != null)
                                {
                                    gazeInteractor.hoverEntered.RemoveAllListeners();
                                    gazeInteractor.hoverExited.RemoveAllListeners();
                                }

                                break;
                        }
                    }
                }
            }
            
            // controllers
            GeneralUIController.Instance.InteractionCreationController.DisableControllerTrigger();
            var triggerActionReference = GeneralUIController.Instance.InteractionCreationController.GetTriggerActionReference();
            triggerActionReference.action.performed -= controllerHandler;
            
            //TODO proximity unbind
            
            gameObjectsWithBindings.Clear();
        }
        
        public void AddBinding(GameObject go, InteractionCreationController.Modalities modality)
        {
            if (!gameObjectsWithBindings.ContainsKey(go))
            {
                gameObjectsWithBindings[go] = new HashSet<InteractionCreationController.Modalities>();
            }
            gameObjectsWithBindings[go].Add(modality);
        }

        public bool HasBinding(GameObject go, InteractionCreationController.Modalities modality)
        {
            return gameObjectsWithBindings.ContainsKey(go) && gameObjectsWithBindings[go].Contains(modality);
        }

        
       

    }
}
