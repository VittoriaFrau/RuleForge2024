using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI.RuleEditor;
using UnityEngine;

namespace UI
{
    public class CombineRulesController:MonoBehaviour
    {
        //Rule composition
        public GameObject removableBarrier;
        private Dictionary<GameObject, Vector3> _originalPositions = new(); //positions of the cubes to easily revert it
        public TextMeshProUGUI whenText, thenText;
        private List<ECAEvent> cubesInRulePlate = new();
        public GameObject ruleEditorPlate;
        private InteractionCreationController interactionCreationController;
        private GeneralUIController generalUIController;
        public GameObject cubePlate, modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant;
        private GameObject whenSequentialRow, whenEquivalenceRow;
        private GameObject thenSequentialRow;
        public enum ContainerType { Equivalence, Sequential }
        public enum RulePhase { When, Then, None }
        private List<CubeContainerClass> whenContainers;
        private List<CubeContainerClass> thenContainers;
        public GameObject modalityContainerPrefab, actionContainerPrefab;
        public GameObject ruleDebugText, cubeHelp;

        
        private void Start()
        {
            interactionCreationController = this.gameObject.GetComponent<InteractionCreationController>();
            generalUIController = this.gameObject.GetComponent<GeneralUIController>();
        }


        public void ActivateCombineRules()
        {
            List<ECAEvent> _modalityEvents = interactionCreationController.ModalityEvents;
            List<ECAEvent> _actionEvents = interactionCreationController.ActionEvents;
            
            if (_modalityEvents.Count == 0 && _actionEvents.Count == 0)
            {
                generalUIController.SetDebugText("No recorded actions, please use the record button to record actions");
                return;
            }
            
            //Set the rule plate visible
            ruleEditorPlate.SetActive(true);
            
            //Barrier to prevent the cubes from falling
            removableBarrier.SetActive(true);

            //Generate the cubes using the list of events
            _originalPositions.Clear();
            
            
            _originalPositions = Utils.GenerateCubesFromEventList(_modalityEvents, _actionEvents, 
                modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant, cubePlate, cubesInRulePlate);

            removableBarrier.SetActive(false);
            
            InitializeVariables();
            
            //Add to the list of created cubes, the cubes that are already in the scene
            cubesInRulePlate.AddRange(_modalityEvents);
            cubesInRulePlate.AddRange(_actionEvents);
        }

        
        public void DeActivateRuleComposition()
        { 
            //Set the rule plate visible
            ruleEditorPlate.SetActive(false);
            
            //Barrier to prevent the cubes from falling
            removableBarrier.SetActive(false);
            
            generalUIController.UIstate = GeneralUIController.UIState.Default;
            generalUIController.DefaultState();
        }
        
        public void ResetCubePositions()
        {
            //Repositioning the plate in case the user has moved it
            ruleEditorPlate.transform.localPosition= new Vector3(-14.4f, -119.0f, 774.0f);
            
            // Using the original position of the cube, position it again there
            foreach (var k in _originalPositions)
            {
                k.Key.transform.position = k.Value;
            }
            Utils.ClearTextDescription(whenText, thenText);

            Utils.ResetCubeContainers();
        }

        
        
        public void AutomaticCubePosition()
        {
            //Modality events
            //Find all the gameobjects with tag RuleCubes and save to modalityCubes if the name contains "Modality"
            List<GameObject> modalityCubes = GameObject.FindGameObjectsWithTag("RuleCubes").Where(obj => obj.name.Contains("Modality")).ToList();
            //Position of the first cube container
            Vector3 firstCubeContainerWhenLocalPosition = new Vector3(144f, 18f,-18f);
            //Lista con i gameobject e l'indice che ne determina l'ordine di creazione dei cubi
            List<Tuple<int, GameObject>> modalityCubesTuple = new();
            
            foreach (ECAEvent modality in interactionCreationController.ModalityEvents)
            {
                foreach (GameObject cube in modalityCubes)
                {
                    if(modality.CubeID == cube.GetInstanceID().ToString())
                        modalityCubesTuple.Add(new Tuple<int, GameObject>(modality.Index, cube));
                }
            }
            
            //Scorro la lista di tuple e ordino i cubi in base all'indice
            modalityCubesTuple = modalityCubesTuple.OrderBy(x => x.Item1).ToList();
            
            float xOffsetBetweenCubes = 50f; // Distanza fissa tra i cubi lungo l'asse x

            
            //Posiziono il primo cubo in firstCubeContainerLocalPosition e i successivi in base alla posizione del precedente
            for (int i = 0; i < modalityCubesTuple.Count; i++)
            {
                GameObject cube = modalityCubesTuple[i].Item2;
                cube.transform.localPosition = firstCubeContainerWhenLocalPosition + new Vector3(i * xOffsetBetweenCubes, 0, 0);
            }
            
            //Action events
            //Find all the gameobjects with tag RuleCubes and save to modalityCubes if the name contains "Action"
            List<GameObject> actionCubes = GameObject.FindGameObjectsWithTag("ActionRuleCube").Where(obj => obj.name.Contains("Action")).ToList();
            //Position of the first cube container
            Vector3 firstCubeContainerThenLocalPosition = new Vector3(144f, -83f,-21f);
            //Lista con i gameobject e l'indice che ne determina l'ordine di creazione dei cubi
            List<Tuple<int, GameObject>> actionCubesTuple = new();
            

            foreach (ECAEvent action in interactionCreationController.ActionEvents)
            {
                foreach (GameObject cube in actionCubes)
                {
                    if(action.CubeID == cube.GetInstanceID().ToString())
                        actionCubesTuple.Add(new Tuple<int, GameObject>(action.Index, cube));
                }
            }
            
            //Scorro la lista di tuple e ordino i cubi in base all'indice
            actionCubesTuple = actionCubesTuple.OrderBy(x => x.Item1).ToList();
            
            //Posiziono il primo cubo in firstCubeContainerLocalPosition e i successivi in base alla posizione del precedente
            for (int i = 0; i < actionCubesTuple.Count; i++)
            {
                GameObject cube = actionCubesTuple[i].Item2;
                cube.transform.localPosition = firstCubeContainerThenLocalPosition + new Vector3(i * xOffsetBetweenCubes, 0, 0);
            }
        }
        //I need a function since it will be called as soon as the rule mode is on
        public void InitializeVariables()
        {
            whenText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x=>x.name=="WhenText").GetComponent<TextMeshProUGUI>();
            thenText = GameObject.FindGameObjectsWithTag("RuleText")
                .ToList().Find(x=>x.name=="ThenText").GetComponent<TextMeshProUGUI>();

            Transform when = GameObject.FindGameObjectsWithTag("RuleUtils").ToList()
                .Find(x => x.name == "When").transform.Find("Frontplate").transform;
            whenSequentialRow = when.Find("SequentialRow").gameObject;
            whenEquivalenceRow = when.Find("EquivalenceRow").gameObject;
            
            Transform then = GameObject.FindGameObjectsWithTag("RuleUtils").ToList()
                .Find(x => x.name == "Then").transform.Find("Frontplate").transform;
            thenSequentialRow = then.Find("SequentialRow").gameObject;
            
            
            //Adds the default containers
            whenContainers = new List<CubeContainerClass>();
            GameObject firstWhenContainer = whenSequentialRow.transform.Find("CubeContainer").gameObject;
            //GameObject firstWhenCube = firstWhenContainer.GetComponent<CubeContainer>().currentCube;
            AddContainer(RulePhase.When, firstWhenContainer);
            thenContainers = new List<CubeContainerClass>();
            GameObject firstThenContainer = thenSequentialRow.transform.Find("ActionCubeContainer").gameObject;
            //GameObject firstThenCube = firstThenContainer.GetComponent<CubeContainer>().currentCube;
            AddContainer(RulePhase.Then, firstThenContainer );
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
        public void CalculateRuleText(GameObject cube, RulePhase rulePhase, bool isAdded, ContainerType containerType, int id)
        {
            UpdatePresentRule(); //Updates the textmeshpro variables with the current rule text
            string cubeDescription = Utils.GetRuleDescriptionFromCubePrefab(cube.gameObject);
            string formattedCubeDescription = cubeDescription.Replace("\n", " ");
            if (isAdded)
            {
                string logicalOperator= containerType == ContainerType.Equivalence ? "OR" : ",";
                Utils.GenerateTextFromCubePosition(rulePhase == RulePhase.When ? whenText : thenText, formattedCubeDescription, logicalOperator);
            }
            else
            {
                string logicalOperator= containerType == ContainerType.Equivalence ? "OR" : ",";
                switch (rulePhase)
                {
                    case RulePhase.When:
                        CubeContainerClass whenContainer = FindContainerById(id, whenContainers);
                        //Look in the when text for the cube description and remove it
                        Utils.RemoveTextFromCubePosition(whenText, formattedCubeDescription, logicalOperator);
                        break;
                    case RulePhase.Then:
                        CubeContainerClass thenContainer = FindContainerById(id, thenContainers);
                        Utils.RemoveTextFromCubePosition(thenText, formattedCubeDescription, logicalOperator);
                        break;
                }
                /*string ruleToRemove = */
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
                if(container!=null) thenContainers.Remove(container);
            }
        }

        private CubeContainerClass FindContainerById(int id, List<CubeContainerClass> list)
        {
            foreach (var cont in list)
            {
                if(cont.id == id) return cont;
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
    

    }
}