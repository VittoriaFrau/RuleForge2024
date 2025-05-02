using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
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
        
        private ObjectsMenuController _objectsMenuController;
        private InteractionCreationController _interactionCreationController;

        private int counter = 0;
        
        private Vector3 initialPosition;

        protected virtual void Awake()
        {
                      // Aggiungi ObjectManipulator se non esiste
            var manipulator = gameObject.GetComponent<ObjectManipulator>();
            if (manipulator == null)
            {
                manipulator = gameObject.AddComponent<ObjectManipulator>();
            }

            // Aggiungi BoxCollider se non esiste
            if (gameObject.GetComponent<BoxCollider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }

            // Configura Rigidbody se non esiste
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            gameRenderer = this.gameObject.GetComponent<Renderer>();
            if(gameRenderer == null)
                gameRenderer = this.gameObject.AddComponent<MeshRenderer>();
            color = gameRenderer.material.color;
            
            var eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            _objectsMenuController = eventHandler.GetComponent<ObjectsMenuController>();
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
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
        /// <b>SpawnObject</b> spawns a new object in the scene.
        /// </summary>
        ///
        [Action(typeof(ECAObject), "duplicates")]
        public void CreateDuplicates(int spawnCount, float delaySeconds)
        {
            StartCoroutine(CreateDuplicatesCoroutine(spawnCount, delaySeconds));
        }

        private IEnumerator CreateDuplicatesCoroutine(int spawnCount, float delaySeconds)
        {
            Debug.Log("Dentro la coroutine");
            Transform floorTransform = GameObject.FindWithTag("Floor").transform;
            Renderer floorRenderer = floorTransform.GetComponent<Renderer>();
            if (floorRenderer == null)
            {
                Debug.LogError("Il Floor non ha un Renderer.");
                yield break;
            }

            Bounds floorBounds = floorRenderer.bounds;
            float minX = floorBounds.min.x;
            float maxX = floorBounds.max.x;
            float minZ = floorBounds.min.z;
            float maxZ = floorBounds.max.z;

            string baseName = this.name.Substring(0, this.name.Length - 1);
            string objCategory = UI.Utils.GetECALastScriptFromECAObject(this.gameObject);

            float margin = 2.0f;

            for (int i = 0; i < spawnCount; i++)
            {
                float randomX = Random.Range(minX + margin, maxX - margin);
                float randomZ = Random.Range(minZ + margin, maxZ - margin);
                Vector3 spawnPosition = new Vector3(randomX, transform.position.y, randomZ);

                _objectsMenuController.Spawn(baseName, spawnPosition, objCategory);

                yield return new WaitForSeconds(delaySeconds);
            }
        }

        
        /// <summary>
        /// <b>Explode</b> spawns a number of fragments from the object.
        /// </summary>
        ///
        [Action(typeof(ECAObject), "explodes")]
        public void Explode()
        {
            int fragments = 10; 
            float explosionForce = 500f; 
            float explosionRadius = 3f;
            initialPosition = transform.position;
            for (int i = 0; i < fragments; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f;
                GameObject frag = Instantiate(this.gameObject, spawnPos, Random.rotation);
                frag.transform.localScale = Vector3.one * 0.05f; // Scale down the fragment
            
                Rigidbody rb = frag.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 explosionDir = (frag.transform.position - transform.position).normalized;
                    rb.AddForce(explosionDir * explosionForce);
                }

                Destroy(frag, 5f); // Auto-destroy after 5 seconds
            }

            gameObject.SetActive(false); 
        }
        
        
        [Action(typeof(ECAObject), "delete duplicates")]
        public void DeleteDuplicates()
        {
            foreach (var obj in _objectsMenuController.spawnedObjects)
            {
                Destroy(obj);
            }
        }
        
        [Action(typeof(ECAObject), "follows")]
        public void Follow()
        {
            GameObject hand = GameObject.FindWithTag("GrabInteractor");
            transform.SetParent(hand.transform);
            
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.MovePosition(hand.transform.position);
            rb.MoveRotation(hand.transform.rotation);
        }
        
        [Action(typeof(ECAObject), "unfollows")]
        public void Unfollow()
        {
            transform.SetParent(_objectsMenuController.interactables.transform);
        }
        
        [Action(typeof(ECAObject), "launches")]
        public void Launch()
        {
            initialPosition = transform.position;
            Unfollow();
            var rb = this.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(transform.forward * 10f, ForceMode.VelocityChange);
        }
        
        [Action(typeof(ECAObject), "resets")]
        public void ResetObject()
        {
            gameObject.SetActive(true);
            transform.position = initialPosition;
            var rb = this.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
		


        



}