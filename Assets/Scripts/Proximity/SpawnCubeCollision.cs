using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UI.RuleEditor;
using Unity.VisualScripting;

namespace UI
{
    // Component attached to the proximity cube
    public class SpawnCubeCollision : MonoBehaviour
    {
        private Material defaultMaterial;
        public Material highlightMaterial;
        public Material recordMaterial;
        public Material proximityMaterial;
        private Renderer rend;
        public GeneralUIController _generalUIController;
        public GameObject screenshotCamera;
        private ScreenshotCamera _screenshotCamera;
        private GameObject ProximityGameObject1, ProximityGameObject2;
        public HandMenuManager handMenuManager;

        public GameObject interactionButtons;
        public GameObject backButton;
        private Vector3 initialPosition;
        private void Awake()
        {
            rend = GetComponent<Renderer>();
            defaultMaterial = rend.material;
        }

        private void Start()
        {
            if (screenshotCamera != null)
            {
                _screenshotCamera = screenshotCamera.GetComponent<ScreenshotCamera>();
            }
            handMenuManager = GameObject.FindGameObjectWithTag("HandMenu").GetComponent<HandMenuManager>();
            initialPosition = transform.localPosition;
        }

        public void ResetToInitialPosition()
        {
            transform.localPosition = initialPosition;
        }
        
        public void ChangeMaterial(String stringMaterial)
        {
            if (rend == null)
            {
                rend = GetComponent<Renderer>();
                defaultMaterial = rend.material;
            }
            switch (stringMaterial)
            {
                case "default":
                    rend.material = defaultMaterial;
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
            String selectedObjectName = GeneralUIController.Instance.GetSelectedObject().name.ToLower();
            if (other.CompareTag("Interactable"))
            {
                ProximityGameObject1 = other.gameObject;
                ChangeMaterial("proximity");
                _generalUIController.SetDebugText("Do you want the new " + selectedObjectName + "s" +
                                                  " to spawn close to the " + ProximityGameObject1.name.ToLower() + "?");
                interactionButtons.SetActive(true);
                backButton.SetActive(false);
                handMenuManager.HideMenus();
                if (GeneralUIController.Instance.isRecording)
                {
                    string screenshotName = selectedObjectName + "spawn near " + ProximityGameObject1.name;
                    _screenshotCamera.SaveImageFromCameraStatic(screenshotCamera.GetComponent<Camera>(), screenshotName);
                    //todo: create the resulting cube
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

