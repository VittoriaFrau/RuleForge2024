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
    // Component attached to the proximity cube
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
        private GameObject ProximityGameObject1, ProximityGameObject2;

        private void Start()
        {
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
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
        

        void OnTriggerEnter(Collider other)
        {
            //add to the list of interacting objects only if the object is an interactable object
            if (other.CompareTag("Interactable"))
            {
                Debug.Log("isrecording: " + GeneralUIController.Instance.isRecording);
                if (GeneralUIController.Instance.isRecording)
                {
                    ChangeMaterial("record");
                    if (other.gameObject != ProximityGameObject1)
                    {
                        ProximityGameObject2 = other.gameObject; //set the object to be used in the rule creation
                        string screenshotName = ProximityGameObject1.name + ProximityGameObject2.name + "collision";
                        _screenshotCamera.SaveImageFromCameraStatic(screenshotCamera.GetComponent<Camera>(), screenshotName);
                        Debug.Log("ProximityGameObject2: " + ProximityGameObject2.name);
                        ChangeMaterial("proximity");
                        _generalUIController.SetDebugText("The " + ProximityGameObject2.name + " is near to the " + ProximityGameObject1.name);
                    }
                }
                else
                {
                    if (ProximityGameObject1 != null) return;
                    ProximityGameObject1 = other.gameObject; //set the object to be used in the rule creation
                    Debug.Log("ProximityGameObject1: " + ProximityGameObject1.name);
                    ChangeMaterial("highlight");

                }
                
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                ChangeMaterial("current");
            }
        }
    
    }

}
