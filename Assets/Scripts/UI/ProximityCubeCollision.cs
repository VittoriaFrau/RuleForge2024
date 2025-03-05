using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UI.RuleEditor;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class ProximityCubeCollision : MonoBehaviour
    {
        private Material currentMaterial;
        public Material highlightMaterial;
        public Material recordMaterial;
        public Material proximityMaterial;
        private Renderer rend;
        public GeneralUIController _generalUIController;
        public GameObject screenshotCamera;
        private ScreenshotCamera _screenshotCamera;
        public Texture2D proximityScreenshot;
        private GameObject eventHandler;
        private InteractionCreationController _interactionCreationController;
        public List<GameObject> interactingObjects;

        private void Start()
        {
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
            _generalUIController = eventHandler.GetComponent<GeneralUIController>();
            rend = GetComponent<Renderer>();
            currentMaterial = rend.material;
        
            if (screenshotCamera != null)
            {
                _screenshotCamera = screenshotCamera.GetComponent<ScreenshotCamera>();
            }
        }
        
        public void ChangeMaterial(String stringMaterial)
        {
            switch (stringMaterial)
            {
                case "current":
                    rend.material = currentMaterial;
                    break;
                case "highlight":
                    rend.material = highlightMaterial;
                    break;
                case "record":
                    rend.material = recordMaterial;
                    break;
                case "proximity":
                    rend.material = proximityMaterial;
                    break;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }

        private void OnCollisionExit(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }

        //TODO
        void OnTriggerEnter(Collider other)
        {
            //add to the list of interacting objects only if the object is an interactable object
            if (other.transform.IsChildOf(_interactionCreationController.interactables.transform))
            {
                interactingObjects.Add(other.gameObject);
                string material = interactingObjects.Count == 1 ? "highlight" :"proximity";
                ChangeMaterial(material);
                if (interactingObjects.Count > 1)
                {
                    _generalUIController.SetDebugText("The " + interactingObjects[1].name + " is near to the " + interactingObjects[0].name);
                    string screenshotName = interactingObjects[1].name + interactingObjects[0].name + "collision";
                    _screenshotCamera.SaveImageFromCameraStatic(screenshotCamera.GetComponent<Camera>(), screenshotName);
                }
            }
            
            /*
            if (other.gameObject.CompareTag("Bird"))
            {
                if(gameObject.name.Equals("Box"))
                {
                    other.gameObject.SetActive(false);
                }
                else
                {
                    //Change the material with highlight material
                    rend.material = highlightMaterial;
                    _generalUIController.SetDebugText("The Bird is near to the Box");
                    _screenshotCamera.SaveImageFromCameraStatic(screenshotCamera.GetComponent<Camera>(), "birdcollision");
                }
           
            }
            */
        }

        void OnTriggerExit(Collider other)
        {
            if (other.transform.IsChildOf(_interactionCreationController.interactables.transform))
            {
                ChangeMaterial("current");
            }
        }
    
    }

}
