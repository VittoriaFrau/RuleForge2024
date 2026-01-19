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

            FindRulePhase(parent);
            Debug.Log($"[CubeContainer] Start on {name}. rulePhase={rulePhase} parent={parent?.name}");
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

            var eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            _combineRulesController = eventHandler != null
                ? eventHandler.GetComponent<CombineRulesController>()
                : FindObjectOfType<CombineRulesController>();
            if (_combineRulesController == null)
            {
                Debug.LogError("[CubeContainer] CombineRulesController not found. Disable CubeContainer.");
                enabled = false;
                return;
            }
            
            modalityContainerPrefab = _combineRulesController.modalityContainerPrefab;
            actionContainerPrefab = _combineRulesController.actionContainerPrefab;
            Debug.Log($"[CubeContainer] Prefabs: modality={modalityContainerPrefab?.name} action={actionContainerPrefab?.name}");
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[CubeContainer] OnCollisionEnter {name} <- {collision.gameObject.name}");
            if (!TryGetCubeRoot(collision.gameObject, out var cubeRoot))
            {
                return;
            }
            TryAttachCubeRoot(cubeRoot);
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
            Debug.Log($"[CubeContainer] OnCollisionExit {name} <- {collision.gameObject.name}");
            if (!TryGetCubeRoot(collision.gameObject, out var cubeRoot))
            {
                return;
            }

            if ((cubeRoot.CompareTag("RuleCubes") || cubeRoot.CompareTag("ActionRuleCube"))
                && !isInstantiating && !isRemoving)
            {
                isRemoving = true;
                Destroy(equivalenceInstantiated);
                Destroy(sequenceInstantiated);
                _combineRulesController.RemoveContainer(rulePhase);
                _combineRulesController.RemoveContainer(rulePhase);
                currentCube = null;
                _combineRulesController.CalculateRuleText(cubeRoot, rulePhase, false, containerType, id);
            }

            StartCoroutine(ResetInstantiation());
            //CalculateRuleText();
        }

        private void PositionGameObjectInContainer(GameObject cubeRoot)
        {
            Debug.Log($"[CubeContainer] Position cube {cubeRoot.name} into {name}");
            //Deactivate object manipulator from object
            if (cubeRoot.TryGetComponent<ObjectManipulator>(out var manipulator))
            {
                manipulator.enabled = false;
            }
                
            //Position the cube in the right position
            Vector3 positionContainer = gameObject.transform.position;
            cubeRoot.transform.position = new Vector3(positionContainer.x, positionContainer.y + 0.1f ,positionContainer.z);

            //Set the collision transform velocities to 0
            if (cubeRoot.TryGetComponent<Rigidbody>(out var body))
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
                
            cubeRoot.transform.rotation = gameObject.transform.rotation;
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
            Debug.Log($"[CubeContainer] Created sequence container {sequenceInstantiated.name} under {transform.parent.name}");
        }

        private void CreateEquivalenceContainer()
        {
            //Modality or action container
            GameObject containerPrefab = rulePhase == CombineRulesController.RulePhase.When ? modalityContainerPrefab : actionContainerPrefab;
            //Create a new instance of the container in the equivalence position
            if (equivalenceCubeContainer == null)
            {
                Debug.LogWarning("[CubeContainer] EquivalenceRow not found, skipping equivalence container creation.");
                return;
            }

            equivalenceInstantiated = Instantiate(containerPrefab);
            equivalenceInstantiated.transform.parent = equivalenceCubeContainer.transform;
            equivalenceInstantiated.transform.rotation = gameObject.transform.rotation;
            equivalenceInstantiated.transform.localScale = transform.localScale;
            var localPosition = equivalenceInstantiated.transform.localPosition;
            localPosition = new Vector3(localPosition.x,  
                localPosition.y,3.99f);
            equivalenceInstantiated.transform.localPosition = localPosition;
            Debug.Log($"[CubeContainer] Created equivalence container {equivalenceInstantiated.name} under {equivalenceCubeContainer.name}");

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            Debug.Log($"[CubeContainer] OnTriggerEnter {name} <- {other.gameObject.name}");
            OnCollisionEnterProxy(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null) return;
            Debug.Log($"[CubeContainer] OnTriggerExit {name} <- {other.gameObject.name}");
            OnCollisionExitProxy(other.gameObject);
        }

        private void OnCollisionEnterProxy(GameObject other)
        {
            Debug.Log($"[CubeContainer] OnCollisionEnterProxy {name} <- {other.name}");
            if (!TryGetCubeRoot(other, out var cubeRoot))
            {
                return;
            }
            TryAttachCubeRoot(cubeRoot);
        }

        private void OnCollisionExitProxy(GameObject other)
        {
            Debug.Log($"[CubeContainer] OnCollisionExitProxy {name} <- {other.name}");
            if (!TryGetCubeRoot(other, out var cubeRoot))
            {
                return;
            }

            if ((cubeRoot.CompareTag("RuleCubes") || cubeRoot.CompareTag("ActionRuleCube"))
                && !isInstantiating && !isRemoving)
            {
                isRemoving = true;
                Destroy(equivalenceInstantiated);
                Destroy(sequenceInstantiated);
                _combineRulesController.RemoveContainer(rulePhase);
                _combineRulesController.RemoveContainer(rulePhase);
                currentCube = null;
                _combineRulesController.CalculateRuleText(cubeRoot, rulePhase, false, containerType, id);
            }

            StartCoroutine(ResetInstantiation());
        }

        private static bool TryGetCubeRoot(GameObject hit, out GameObject cubeRoot)
        {
            cubeRoot = null;
            if (hit == null) return false;

            var cubeController = hit.GetComponentInParent<CubeController>();
            if (cubeController != null)
            {
                cubeRoot = cubeController.gameObject;
                Debug.Log($"[CubeContainer] TryGetCubeRoot resolved {hit.name} -> {cubeRoot.name} (CubeController)");
                return true;
            }

            if (hit.CompareTag("RuleCubes") || hit.CompareTag("ActionRuleCube"))
            {
                cubeRoot = hit;
                Debug.Log($"[CubeContainer] TryGetCubeRoot resolved {hit.name} by tag");
                return true;
            }

            Debug.Log($"[CubeContainer] TryGetCubeRoot failed for {hit.name}");
            return false;
        }

        public bool TryAttachCube(GameObject cubeRoot)
        {
            if (cubeRoot == null || !enabled)
            {
                return false;
            }

            return TryAttachCubeRoot(cubeRoot);
        }

        private bool TryAttachCubeRoot(GameObject cubeRoot)
        {
            //if the cube is a action cube and the container is a when container, return and viceversa
            if (cubeRoot.CompareTag("ActionRuleCube") && rulePhase == CombineRulesController.RulePhase.When)
            {
                return false;
            }
            
            if (cubeRoot.CompareTag("RuleCubes") && rulePhase == CombineRulesController.RulePhase.Then)
            {
                return false;
            }
            
            if(cubeRoot == currentCube)
            {
                // If the current cube is the same as the colliding cube, do nothing
                return false;
            }
            
            // Check if collision occurred with a RuleCube and not already instantiating
            if ((cubeRoot.CompareTag("RuleCubes") || cubeRoot.CompareTag("ActionRuleCube")) && !isInstantiating)
            {
                Debug.Log($"[CubeContainer] Attaching cube {cubeRoot.name} to {name}");
                isInstantiating = true;
                _combineRulesController.DeactivateRuleDebugText();
                PositionGameObjectInContainer(cubeRoot);
                CreateSequenceContainer();
                _combineRulesController.AddContainer(rulePhase, this.gameObject);

                if (rulePhase != CombineRulesController.RulePhase.Then)
                {
                    CreateEquivalenceContainer();
                    _combineRulesController.AddContainer(rulePhase, gameObject);
                }
                
                currentCube = cubeRoot;

                if (cubeRoot.TryGetComponent<ObjectManipulator>(out var manipulator))
                {
                    manipulator.enabled = true;
                }
                
                //Update text
                _combineRulesController.CalculateRuleText(cubeRoot, rulePhase, true, containerType, id );
                StartCoroutine(ResetInstantiation());
                return true;
            }

            return false;
        }
    }
}
