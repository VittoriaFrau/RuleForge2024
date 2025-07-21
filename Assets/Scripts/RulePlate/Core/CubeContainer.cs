using System;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UnityEngine;

namespace UI.RuleEditor
{
    public class CubeContainerClass
    {
        public int id;
        public CombineRulesController.ContainerType containerType;
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
    public class CubeContainer:MonoBehaviour
    {
        public int id; // Unique identifier of the container
        private GameObject modalityContainerPrefab, actionContainerPrefab;  // Prefab of the cube container to be instantiated
        private bool isInstantiating, isRemoving;   // Flag to prevent multiple instantiations on collision
        public CombineRulesController.RulePhase rulePhase;    // Indicates whether the container belongs to "when" or "then" branch
        private GameObject equivalenceCubeContainer;   // Reference to the equivalence container for each cube container
        private string whenString, thenString;  // Current text contents of the "when" and "then" text objects
        public CombineRulesController.ContainerType containerType;    // Indicates whether the container is equivalence (OR) or sequential
        private GameObject sequenceInstantiated, equivalenceInstantiated; // References to the instantiated containers
        private CombineRulesController _combineRulesController;        // Reference to the CombineRulesController script
        public GameObject currentCube=null; //reference to the cube the gameobject contains
        
        private void Start()
        {
            //find if this instance is a child of a then or when
            Transform parent = gameObject.transform.parent;

            if (rulePhase == CombineRulesController.RulePhase.None)
            {
                FindRulePhase(parent);
            }
            parent = gameObject.transform.parent;
            // Find the parent EquivalenceRow container
            foreach (Transform child in parent.parent){
                if (child.name == "EquivalenceRow"){
                    equivalenceCubeContainer = child.gameObject;
                    break;
                }
            }

            containerType = transform.parent.name switch
            {
                //Check if the parent of collision.gameobject is an equivalence or sequential container
                "EquivalenceRow" => CombineRulesController.ContainerType.Equivalence,
                "SequentialRow" => CombineRulesController.ContainerType.Sequential,
                _ => containerType
            };

            _combineRulesController = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<CombineRulesController>();
            
            modalityContainerPrefab = _combineRulesController.modalityContainerPrefab;
            actionContainerPrefab = _combineRulesController.actionContainerPrefab;
        }

        private void OnCollisionEnter(Collision collision)
        {
            //if the cube is a action cube and the container is a when container, return and viceversa
            if (collision.gameObject.CompareTag("ActionRuleCube") && rulePhase == CombineRulesController.RulePhase.When)
            {
                return;
            }
            
            if (collision.gameObject.CompareTag("RuleCubes") && rulePhase == CombineRulesController.RulePhase.Then)
            {
                return;
            }
            
            if(collision.gameObject == currentCube)
            {
                // If the current cube is the same as the colliding cube, do nothing
                return;
            }
            
            // Check if collision occurred with a RuleCube and not already instantiating
            if ((collision.gameObject.CompareTag("RuleCubes") || collision.gameObject.CompareTag("ActionRuleCube")) && !isInstantiating)
            {
                isInstantiating = true;
                _combineRulesController.DeactivateRuleDebugText();
                PositionGameObjectInContainer(collision);
                CreateSequenceContainer();
                _combineRulesController.AddContainer(rulePhase, this.gameObject);

                if (rulePhase != CombineRulesController.RulePhase.Then)
                {
                    CreateEquivalenceContainer();
                    _combineRulesController.AddContainer(rulePhase, gameObject);
                }
                
                currentCube = collision.gameObject;

                collision.gameObject.GetComponent<ObjectManipulator>().enabled = true;
                
                //Update text
                _combineRulesController.CalculateRuleText(collision.gameObject, rulePhase, true, containerType, id );
                StartCoroutine(ResetInstantiation());
            }
        }

        private void FindRulePhase(Transform parent)
        {
            while (parent != null)
            {
                if (parent.name == "When")
                {
                    rulePhase = CombineRulesController.RulePhase.When;
                    break;
                } if (parent.name == "Then")
                {
                    rulePhase = CombineRulesController.RulePhase.Then; 
                }
                parent = parent.parent;
            }
        }
        
        private System.Collections.IEnumerator ResetInstantiation()
        {
            yield return new WaitForSeconds(1f);
            isRemoving = false;
            isInstantiating = false;
        }

        

        private void OnCollisionExit(Collision collision)
        {
            if ((collision.gameObject.CompareTag("RuleCubes") || collision.gameObject.CompareTag("ActionRuleCube"))
                && !isInstantiating && !isRemoving)
            {
                isRemoving = true;
                Destroy(equivalenceInstantiated);
                Destroy(sequenceInstantiated);
                _combineRulesController.RemoveContainer(rulePhase);
                _combineRulesController.RemoveContainer(rulePhase);
                currentCube = null;
                _combineRulesController.CalculateRuleText(collision.gameObject, rulePhase, false, containerType, id);
            }

            StartCoroutine(ResetInstantiation());
            //CalculateRuleText();
        }

        private void PositionGameObjectInContainer(Collision collision)
        {
            //Deactivate object manipulator from object
            collision.gameObject.GetComponent<ObjectManipulator>().enabled = false;
                
            //Position the cube in the right position
            Vector3 positionContainer = gameObject.transform.position;
            collision.gameObject.transform.position = new Vector3(positionContainer.x, positionContainer.y + 0.1f ,positionContainer.z);

            //Set the collision transform velocities to 0
            collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            collision.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                
            collision.gameObject.transform.rotation = gameObject.transform.rotation;
        }

        private void CreateSequenceContainer()
        {
            //Modality or action container
            GameObject containerPrefab = rulePhase == CombineRulesController.RulePhase.When ? modalityContainerPrefab : actionContainerPrefab;
            //Create a new instance of the container in the sequence position
            sequenceInstantiated = Instantiate(containerPrefab);
            sequenceInstantiated.transform.parent = transform.parent;
            sequenceInstantiated.transform.rotation = gameObject.transform.rotation;
            sequenceInstantiated.transform.localScale = transform.localScale;
            sequenceInstantiated.transform.localPosition = new Vector3(sequenceInstantiated.transform.localPosition.x,  
                sequenceInstantiated.transform.localPosition.y,3.99f);
        }

        private void CreateEquivalenceContainer()
        {
            //Modality or action container
            GameObject containerPrefab = rulePhase == CombineRulesController.RulePhase.When ? modalityContainerPrefab : actionContainerPrefab;
            //Create a new instance of the container in the equivalence position
            equivalenceInstantiated = Instantiate(containerPrefab);
            equivalenceInstantiated.transform.parent = equivalenceCubeContainer.transform;
            equivalenceInstantiated.transform.rotation = gameObject.transform.rotation;
            equivalenceInstantiated.transform.localScale = transform.localScale;
            var localPosition = equivalenceInstantiated.transform.localPosition;
            localPosition = new Vector3(localPosition.x,  
                localPosition.y,3.99f);
            equivalenceInstantiated.transform.localPosition = localPosition;

        }
    }
}