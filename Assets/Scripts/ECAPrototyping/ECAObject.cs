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
using UI.RuleEditor;
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
        
        [StateVariable("duplicate", ECARules4AllType.Boolean)] 
        public ECABoolean isDuplicated = new(ECABoolean.BoolType.NO);
        
        private ObjectsMenuController _objectsMenuController;
        private GeneralUIController _generalUIController;

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
            _generalUIController = eventHandler.GetComponent<GeneralUIController>();
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
        [Action(typeof(ECAObject), "is duplicated into" , typeof(int))]
        public void CreateDuplicates(int spawnCount)
        {
            Renderer[] renderers = this.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.material = GeneralUIController.Instance.spawnMaterial; // update material 
            string baseName = this.name.Substring(0, this.name.Length - 1);
            string objCategory = UI.Utils.GetECALastScriptFromECAObject(this.gameObject);

            float margin = 2.0f;

            // Width of the base object
            Bounds originalBounds = GetComponent<Renderer>().bounds;
            float objectWidth = originalBounds.size.x;
            float spacing = objectWidth + margin;

            Vector3 basePosition = transform.position;

            for (int i = 0; i < spawnCount; i++)
            {
                //with this method we can spawn the objects in a line on the right
                Vector3 spawnPosition = basePosition + new Vector3(spacing * (i + 1), 0, 0); 
                GameObject duplicate = _objectsMenuController.Spawn(baseName, spawnPosition, objCategory);

                Renderer[] duplicateRenderers = duplicate.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in duplicateRenderers)
                    r.material = GeneralUIController.Instance.spawnMaterial;
            }

            isDuplicated.Assign(ECABoolean.BoolType.YES);
            
            this.gameObject.name = "new" + baseName;
        }
        
        /// <summary>
        /// <b>DeleteDuplicates</b> deletes all the duplicates of the object in the scene.
        /// </summary>
        ///
        [Action(typeof(ECAObject), "delete duplicates")]
        public void DeleteDuplicates()
        {
            foreach (var obj in _objectsMenuController.spawnedObjects)
            {
                Destroy(obj);
            }
            // Restore original material if it was changed of the object
            Material originalMat = _generalUIController.GetOriginalMaterial(this.gameObject);
            if (originalMat != null)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
                foreach (Renderer rend in renderers)
                {
                    rend.material = originalMat;
                }
            }
            isDuplicated.Assign(ECABoolean.BoolType.NO);
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
        
        
        /// <summary>
        /// <b>Follow</b> makes the object follow the hand.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "follows")]
        public void Follow()
        {
            initialPosition = transform.position;
            GameObject hand = GameObject.FindWithTag("GrabInteractor");
            transform.SetParent(hand.transform);
            
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.MovePosition(hand.transform.position);
            rb.MoveRotation(hand.transform.rotation);
        }
        
        /// <summary>
        /// <b>Unfollow</b> detaches the object from the hand and place it back into the interactables.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "unfollows")]
        public void Unfollow()
        {
            transform.SetParent(_objectsMenuController.interactables.transform);
            ResetObject();
        }
        
        /// <summary>
        /// <b>Launch</b> makes the object move it forward at some speed.
        /// </summary>
        /// 
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
        
        /// <summary>
        /// <b>ResetObject</b> restore the initial properties of the object.
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "resets")]
        public void ResetObject()
        {
            Vector3 initialPos = _generalUIController.GetInitialPosition(this.gameObject);
            gameObject.transform.position = initialPos;
            gameObject.SetActive(true);

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }



        /// <summary>
        /// <b>Moves</b> moves the selected object close to the spawn cube.   
        /// </summary>
        /// 
        [Action(typeof(ECAObject), "moves")]
        public void Moves()
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
        }
    }
}