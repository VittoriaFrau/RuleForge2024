using System.Collections.Generic;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.SpatialManipulation;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;


namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// <b>ECAObject</b> is the base class for all objects that can be used in the rule engine.
    /// All the other classes in this package inherit from this class or one of its subclasses.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("object")]

    public class ECAObject : MonoBehaviour
    {
        /// <summary>
        /// <b>GameRender</b> is the renderer of the object.
        /// </summary>
        private Renderer gameRenderer;
        private ObjectManipulator objectManipulator;
        private Rigidbody rigidbody;
        private BoxCollider boxCollider;
        
        /// <summary>
        /// <b> Color </b> is the color of the object 
        /// </summary>
        [StateVariable("color", ECARules4AllType.Color)] 
        public Color color;

        /// <summary>
        /// <b>isVisible</b> is a boolean that indicates if the object is visible.
        /// If the object is invisible, it will not be rendered but it will still collide with other objects.
        /// </summary>
        [StateVariable("visible", ECARules4AllType.Boolean)] 
        public ECABoolean isVisible = new (ECABoolean.BoolType.YES);
        
        /// <summary>
        /// <b> Gravity </b> is a boolean that indicates if the object is affected by gravity.
        /// </summary>
        [StateVariable("gravity", ECARules4AllType.Boolean)] 
        public ECABoolean isUsingGravity = new(ECABoolean.BoolType.YES);
        
        [StateVariable("duplicate", ECARules4AllType.Boolean)] 
        public ECABoolean isDuplicated = new(ECABoolean.BoolType.NO);
        
        public ECABoolean isACopy = new(ECABoolean.BoolType.NO);
        
        public ECABoolean shouldFloat = new(ECABoolean.BoolType.NO);
        
        private int counter = 0;
        private Vector3 initialPosition;
        private Quaternion initialRot => transform.rotation; // Store the initial rotation of the object
        
        // Store original materials for this object and its children
        private Material originalMaterial;
        private Dictionary<Renderer, Material[]> childOriginalMaterials = new();

        protected virtual void Awake()
        {
            // Aggiungi ObjectManipulator se non esiste
            objectManipulator = gameObject.GetComponent<ObjectManipulator>();
            if (objectManipulator == null)
            {
                objectManipulator = gameObject.AddComponent<ObjectManipulator>();
            }

            // Aggiungi BoxCollider se non esiste
            boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            // Configura Rigidbody se non esiste
            rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
            }

            if (shouldFloat)
            {
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                if (gameObject.name.ToLower().Contains("timer"))
                {
                    //unfreeze the position on the Y axis
                    rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionX;
                }
                
            }

        gameRenderer = this.gameObject.GetComponent<Renderer>();
            if(gameRenderer == null)
                gameRenderer = this.gameObject.AddComponent<MeshRenderer>();
            color = gameRenderer.material.color;
            
            SaveOriginalMaterials();
        }
        

        /// <summary>
        /// <b>Shows</b> makes the object visible. It makes it visible if it is not already.
        /// </summary>
        [Action(typeof(ECAObject), "shows")]
        public void Shows()
        {
            //check if the object is already visible
            /*if (this.gameObject.activeInHierarchy)
            {
                isVisible.Assign(ECABoolean.BoolType.YES);
            }
            else
            {
                isVisible.Assign(ECABoolean.BoolType.NO);
            }*/
            isVisible.Assign(ECABoolean.BoolType.YES);
            UpdateVisibility();
        }
        
        
        /// <summary>
        /// <b>Hides</b> makes the object invisible. It makes it invisible if it is not already.
        /// </summary>
        [Action(typeof(ECAObject), "hides")]
        public void Hides()
        {
            isVisible.Assign(ECABoolean.BoolType.NO);
            UpdateVisibility();
        }
        
        
        /// <summary>
        /// <b>ShowsHides</b> sets the visibility of the object, defined by a parameter.
        /// </summary>
        /// <param name="yesNo">The new visibility state for the object. </param>
        [Action(typeof(ECAObject), "changes", "visible", "to", typeof(YesNo))]
        public void ShowsHides(ECABoolean yesNo)
        {
            isVisible = yesNo;
            UpdateVisibility();
        }
        
        private void UpdateVisibility()
        {
            this.gameObject.SetActive(isVisible);
        }
        
        
        /// <summary>
        /// <b>SetsColor</b> sets the color of the light source to the given value.
        /// </summary>
        /// <param name="inputColor">The new color value.</param>
        [Action(typeof(ECAObject), "changes","color", "to", typeof(ECAColor))]
        public void ChangesColor(ECAColor inputColor)
        {
            color = inputColor.Color;
            if (this.gameObject.GetComponent<Light>() != null)
            {
                this.gameObject.GetComponent<Light>().color = color;
            }
            else
            {
                gameRenderer.material.color = color;
            }
        }
            
        
        
        public void ChangeColor(string newColor)
        {
            //convert string to color
            ColorUtility.TryParseHtmlString(newColor, out color);
            gameRenderer.material.color = color;
        }
        
        /// <summary>
        /// <b>GravityON</b> sets to true the gravity.
        [Action(typeof(ECAObject), "gravityON")]
        public void GravityON()
        {
            isUsingGravity = ECABoolean.YES;
            UpdateGravity();
        }
        
        /// <summary>
        /// <b>GravityOFF</b> sets to false the gravity.
        [Action(typeof(ECAObject), "gravityOFF")]
        public void GravityOFF()
        {
            isUsingGravity = ECABoolean.NO;
            UpdateGravity();
        }

        /// <summary>
        /// <b>UpdateGravity</b> updates the gravity of the object.
        /// </summary>
        public void UpdateGravity()
        {
            switch (isUsingGravity.GetBoolType())
            {
               case ECABoolean.BoolType.YES:
                   gameObject.GetComponent<Rigidbody>().useGravity = true;
                   break;
               case ECABoolean.BoolType.NO:
                   gameObject.GetComponent<Rigidbody>().useGravity = false;
                   break;
            }
        }

        /// <summary>
        /// <b>delete_object</b> deletes the object from the scene.
        /// </summary>
        /// <param name="obj"> is the object to delete from the scene </param>
        [Action(typeof(ECAObject), "is deleted")]
        public void DeleteObject()
        {
            Destroy(this.gameObject);
            
        }

        /// <summary>
        /// <b>Duplicates</b> the object into N objects
        /// </summary>
        ///
        [Action(typeof(ECAObject), "is duplicated" , typeof(int))]
        public void CreateDuplicates(int spawnCount)
        {
            if (GeneralUIController.Instance.UIstate != GeneralUIController.UIState.Play)
            {
                Renderer[] renderers = this.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                    r.material = GeneralUIController.Instance.spawnMaterial; // update material 
            }

            string baseName = this.name.Substring(0, this.name.Length - 1);
            if(name.StartsWith("new"))
            {
                baseName = name.Substring(3); // remove "new" prefix if it exists
            }
            string objCategory = UI.Utils.GetECALastScriptFromECAObject(this.gameObject);

            float spacing = 0.3f;
            Vector3 basePosition = transform.position;

            for (int i = 0; i < spawnCount; i++)
            {
                //with this method we can spawn the objects in a line on the right
                Vector3 spawnPosition = new Vector3(spacing * i, basePosition.y, basePosition.z);
                GameObject duplicate = GeneralUIController.Instance.ObjectsMenuController.Spawn(baseName, spawnPosition, objCategory);
                duplicate.GetComponent<ECAObject>().isACopy.Assign(ECABoolean.BoolType.YES);

                if (GeneralUIController.Instance.UIstate != GeneralUIController.UIState.Play)
                {
                    Renderer[] duplicateRenderers = duplicate.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in duplicateRenderers)
                        r.material = GeneralUIController.Instance.spawnMaterial;
                    //in edit mode we need to add the listener to edit the object
                    GeneralUIController.Instance.EditModeController.AddListenerToSingleInteractable(duplicate);
                }

                //if it's the first duplicate, we set the name to "new" + baseName
                if (GeneralUIController.Instance.ObjectsMenuController.spawnedObjects.Count == 1)
                {
                    duplicate.name = "new" + baseName;
                }else 
                {
                    //if it's not the first duplicate, we set the name to "new" + baseName + counter
                    duplicate.name = "new" + baseName + (GeneralUIController.Instance.ObjectsMenuController.spawnedObjects.Count - 1);
                }
                
                //UI.Utils.CopyMissingComponents(gameObject, duplicate, GeneralUIController.Instance.CombineRulesController.lastECAEvent,
                //GeneralUIController.Instance.CombineRulesController.notifyTracker);
                var lastEvent = GeneralUIController.Instance.CombineRulesController.lastECAEvent;
                var trackerDict = GeneralUIController.Instance.CombineRulesController.EventTrackers;

                if (trackerDict.TryGetValue(lastEvent, out var tracker))
                {
                    UI.Utils.CopyMissingComponents(gameObject, duplicate, lastEvent, tracker.EventTriggered);
                }
                else
                {
                    Debug.LogWarning("No tracker found for lastECAEvent during duplication.");
                }

            }

            isDuplicated.Assign(ECABoolean.BoolType.YES);
        }
        
        /// <summary>
        /// <b>DeleteDuplicates</b> deletes all the duplicates of the object in the scene.
        /// </summary>
        ///
        [Action(typeof(ECAObject), "delete duplicates")]
        public void DeleteDuplicates()
        {
            foreach (var obj in GeneralUIController.Instance.ObjectsMenuController.spawnedObjects)
            {
                Destroy(obj);
            }
            // Restore original material if it was changed of the object
            RestoreOriginalMaterials();
            isDuplicated.Assign(ECABoolean.BoolType.NO);
            GeneralUIController.Instance.ObjectsMenuController.spawnedObjects.Clear();
        }
        
        /// <summary>
        /// <b>Explode</b> spawns a number of fragments from the object.
        /// </summary>
        ///
        [Action(typeof(ECAObject), "explodes")]
        public void Explode()
        {
            int fragments = 10; 
            float explosionForce = 250f; 
            initialPosition = transform.position;
            for (int i = 0; i < fragments; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f;
                GameObject frag = Instantiate(this.gameObject, spawnPos, Random.rotation);
                frag.transform.localScale = transform.localScale * 0.1f; // Scale down the fragment
            
                Rigidbody rb = frag.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 explosionDir = (frag.transform.position - transform.position).normalized;
                    rb.AddForce(explosionDir * explosionForce);
                }

                Destroy(frag, 1f); // Auto-destroy after 5 seconds
            }

            gameObject.SetActive(false); 
        }
        
        
        /// <summary>
        /// <b>Follow</b> makes the object follow the hand.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "follows")]
        public void Follow()
        {
            initialPosition = transform.position;
            //If we are using the controllers I substitute the mesh
            if (GeneralUIController.Instance.UIstate != GeneralUIController.UIState.Play)
            {
                GeneralUIController.Instance.InteractionCreationController.ReplaceLeftControllerModel(gameObject.transform);
                if (UI.Utils.IsEquipable(gameObject))
                {
                    // adjustments for the demo
                    transform.localPosition = new Vector3(-7.53674394e-05f, 0.000259717082f, -5.92828146e-05f);
                    transform.localRotation = Quaternion.Euler(88.7832413f,209.190033f,209.371323f);
                    transform.localScale = new Vector3(0.17f, 0.17f, 0.17f);
                    
                    // Freeze the rotation
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }
                }
            }
            else
            {
                GameObject hand = GameObject.FindWithTag("GrabInteractor");
                transform.SetParent(hand.transform);
            
                var rb = GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.MovePosition(hand.transform.position);
                rb.MoveRotation(hand.transform.rotation);
            }
            
        }
        
        /// <summary>
        /// <b>Unfollow</b> detaches the object from the hand and place it back into the interactables.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "unfollows")]
        public void Unfollow()
        {
            transform.SetParent(GeneralUIController.Instance.ObjectsMenuController.interactables.transform);
            ResetObject();
        }
        
        /// <summary>
        /// <b>Launch</b> makes the object move it forward at some speed.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "is thrown")]
        public void Launch()
        {
            initialPosition = transform.position;
            Unfollow();

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            // Angolo di lancio in gradi
            float launchAngle = 45f;
            float launchSpeed = 10f;

            // Calcola la direzione del lancio in base all'angolo
            Vector3 launchDirection = Quaternion.Euler(-launchAngle, 0, 0) * transform.forward;

            // Applica la forza
            rb.velocity = launchDirection.normalized * launchSpeed;
        }

        
        /// <summary>
        /// <b>ResetObject</b> restore the initial properties of the object.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "resets")]
        public void ResetObject()
        {
            Vector3 initialPos = GeneralUIController.Instance.GetInitialPosition(this.gameObject);
            gameObject.transform.position = initialPos;
            gameObject.transform.rotation = initialRot;
            gameObject.SetActive(true);

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }



        /// <summary>
        /// <b>Moves</b> moves the selected object close to the spawn cube.   
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "moves near", typeof(ECAObject))]
        public void Moves(ECAObject targetObject)
        {
            initialPosition = transform.position;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            GameObject spawnCube = GameObject.FindGameObjectWithTag("SpawnCube");
            if (spawnCube)
            {
                Transform spawnCubeChild = spawnCube.transform.GetChild(0);
                if (spawnCubeChild != null)
                {
                    Vector3 targetPosition = spawnCubeChild.position;
                    transform.position = targetPosition;
                    rb.MovePosition(targetPosition);
                }
            }
            else
            {
                if (targetObject.name.Equals("Floor"))
                {
                    // If the target object is the floor, we just move randomly on the floor
                    Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                    Vector3 upwardOffset = Vector3.up * 1f;
                    Vector3 forwardOffset = mainCamera.transform.forward * 1f;
                    Vector3 spawnPosition = targetObject.transform.position + upwardOffset + forwardOffset;
                    transform.position = spawnPosition;
                }
                // If no spawn cube is found, just move to the target object's position
                else transform.position = targetObject.transform.position;
            }
        }
        
        /// <summary>
        /// <b>Moves</b> moves the selected object close to the spawn cube.   
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "moves around", typeof(ECAObject))]
        public void MovesAround(ECAObject targetObject)
        {
            initialPosition = transform.position;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            GameObject spawnCube = GameObject.FindGameObjectWithTag("SpawnCube");
            if (spawnCube)
            {
                Transform spawnCubeChild = spawnCube.transform.GetChild(0);
                if (spawnCubeChild != null)
                {
                    Vector3 targetPosition = spawnCubeChild.position;
                    transform.position = targetPosition;
                    rb.MovePosition(targetPosition);
                }
            }
            else
            {
                if (targetObject.name.Equals("Floor"))
                {
                    Vector3 spawnPosition =
                        UI.Utils.CalculateRandomSpawnPosition(targetObject.transform.position, gameObject.transform.parent);
                    // Set the object's position
                    transform.position = spawnPosition;
                }
                // If no spawn cube is found, just move to the target object's position
                else transform.position = targetObject.transform.position;
            }
        }
        
        /// <summary>
        /// Saves the original material of this object and all child renderers.
        /// </summary>
        private void SaveOriginalMaterials()
        {
            Renderer thisRenderer = GetComponent<Renderer>();
            if (thisRenderer != null)
            {
                originalMaterial = thisRenderer.sharedMaterial;
            }

            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in childRenderers)
            {
                if (!childOriginalMaterials.ContainsKey(renderer))
                {
                    childOriginalMaterials[renderer] = renderer.sharedMaterials;
                }
            }
        }

        /// <summary>
        /// Restores the original material to this object and all child renderers.
        /// </summary>
        public void RestoreOriginalMaterials()
        {
            Renderer thisRenderer = GetComponent<Renderer>();
            if (thisRenderer != null && originalMaterial != null)
            {
                thisRenderer.material = originalMaterial;
            }

            foreach (var pair in childOriginalMaterials)
            {
                Renderer renderer = pair.Key;
                Material[] originalMats = pair.Value;

                if (renderer != null)
                {
                    renderer.materials = originalMats;
                }
            }
        }
    }
}