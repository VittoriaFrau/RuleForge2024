using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI.RuleEditor
{
    public class CubeContainerClass
    {
        public int id;
        public RuleManager.ContainerType containerType;
        public CubeContainer cubeContainer;
        public GameObject containerGo, cubeGo;

        public CubeContainerClass(int id, CubeContainer cubeContainer)
        {
            this.id = id;
            this.containerType = cubeContainer.containerType;
            containerGo = cubeContainer.gameObject;
            cubeGo = cubeContainer.currentCube;
        }
    }
    public class RuleManager:MonoBehaviour
    {
        private TextMeshProUGUI whenText, thenText;
        private GameObject whenSequentialRow, whenEquivalenceRow;
        private GameObject thenSequentialRow, thenEquivalenceRow;
        public enum ContainerType { Equivalence, Sequential }
        public enum RulePhase { When, Then, None }
        private List<CubeContainerClass> whenContainers;
        private List<CubeContainerClass> thenContainers;
        public GameObject modalityContainerPrefab, actionContainerPrefab;
        
        
        //Tasks
        public List<GameObject> tasks;
        private int currentTask=0;

        public GameObject ruleDebugText, cubeHelp;

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
            thenEquivalenceRow = then.Find("EquivalenceRow").gameObject;
            
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
                string logicalOperator= containerType == ContainerType.Equivalence ? "OR" : "AND THEN";
                Utils.GenerateTextFromCubePosition(rulePhase == RulePhase.When ? whenText : thenText, formattedCubeDescription, logicalOperator);
            }
            else
            {
                string logicalOperator= containerType == ContainerType.Equivalence ? "OR" : "AND THEN";
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