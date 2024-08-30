using System;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UnityEngine;

namespace UI.RuleEditor
{
    public class CubeContainer:MonoBehaviour
    {
        public int id; // Unique identifier of the container
        private GameObject modalityContainerPrefab, actionContainerPrefab;  // Prefab of the cube container to be instantiated
        private bool isInstantiating, isRemoving;   // Flag to prevent multiple instantiations on collision
        public RuleManager.RulePhase rulePhase;    // Indicates whether the container belongs to "when" or "then" branch
        private GameObject equivalenceCubeContainer;   // Reference to the equivalence container for each cube container
        private string whenString, thenString;  // Current text contents of the "when" and "then" text objects
        public RuleManager.ContainerType containerType;    // Indicates whether the container is equivalence (OR) or sequential
        private GameObject sequenceInstantiated, equivalenceInstantiated; // References to the instantiated containers
        private RuleManager ruleManager;        // Reference to the RuleManager script
        public GameObject currentCube=null; //reference to the cube the gameobject contains
        
        private void Start()
        {
            //find if this instance is a child of a then or when
            Transform parent = gameObject.transform.parent;

            if (rulePhase == RuleManager.RulePhase.None)
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
                "EquivalenceRow" => RuleManager.ContainerType.Equivalence,
                "SequentialRow" => RuleManager.ContainerType.Sequential,
                _ => containerType
            };

            ruleManager = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<RuleManager>();
            
            modalityContainerPrefab = ruleManager.modalityContainerPrefab;
            actionContainerPrefab = ruleManager.actionContainerPrefab;
        }

        private void OnCollisionEnter(Collision collision)
        {
            //if the cube is a action cube and the container is a when container, return and viceversa
            if (collision.gameObject.CompareTag("ActionRuleCube") && rulePhase == RuleManager.RulePhase.When)
            {
                return;
            }
            
            if (collision.gameObject.CompareTag("RuleCubes") && rulePhase == RuleManager.RulePhase.Then)
            {
                return;
            }
            
            
            // Check if collision occurred with a RuleCube and not already instantiating
            if ((collision.gameObject.CompareTag("RuleCubes") || collision.gameObject.CompareTag("ActionRuleCube")) && !isInstantiating)
            {
                isInstantiating = true;
                ruleManager.DeactivateRuleDebugText();
                PositionGameObjectInContainer(collision);
                
                CreateSequenceContainer();
                ruleManager.AddContainer(rulePhase, this.gameObject);

                CreateEquivalenceContainer();
                ruleManager.AddContainer(rulePhase, gameObject);

                currentCube = collision.gameObject;

                collision.gameObject.GetComponent<ObjectManipulator>().enabled = true;
                
                //Update text
                ruleManager.CalculateRuleText(collision.gameObject, rulePhase, true, containerType, id );
                
                StartCoroutine(ResetInstantiation());
                
            }
            
            
        }

        private void FindRulePhase(Transform parent)
        {
            while (parent != null)
            {
                if (parent.name == "When")
                {
                    rulePhase = RuleManager.RulePhase.When;
                    break;
                } if (parent.name == "Then")
                {
                    rulePhase = RuleManager.RulePhase.Then; 
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
                ruleManager.RemoveContainer(rulePhase);
                ruleManager.RemoveContainer(rulePhase);
                currentCube = null;
                ruleManager.CalculateRuleText(collision.gameObject, rulePhase, false, containerType, id);
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
            GameObject containerPrefab = rulePhase == RuleManager.RulePhase.When ? modalityContainerPrefab : actionContainerPrefab;
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
            GameObject containerPrefab = rulePhase == RuleManager.RulePhase.When ? modalityContainerPrefab : actionContainerPrefab;
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