using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECAPrototyping.RuleEngine;
using UI;
using UI.RuleEditor;
using Unity.VisualScripting;
using Action = System.Action;

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
        public EditModeController _editModeController;
        public GameObject screenshotCamera;
        private ScreenshotCamera _screenshotCamera;
        public GameObject ProximityGameObject1, ProximityGameObject2;
        public HandMenuManager handMenuManager;

        public GameObject interactionButtons;
        public GameObject backButton;
        private Vector3 initialPosition;
        private string selectedObjBaseName;
        private GameObject selectedObject => GeneralUIController.Instance.GetSelectedObject();
        
        private RuleEngine _ruleEngine;
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
            _ruleEngine = RuleEngine.GetInstance();
            
            selectedObjBaseName = selectedObject.name.Substring(0, name.Length).ToLower();
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
            if (other.CompareTag("Interactable") && other.gameObject != selectedObject)
            {
                ProximityGameObject1 = other.gameObject;
                string proximityObjectName = ProximityGameObject1.name.Substring(0, 
                    ProximityGameObject1.name.Length-1).ToLower();
                ChangeMaterial("proximity");
                GeneralUIController.Instance.SetDebugText("Do you want the " + selectedObjBaseName + 
                                                  " to move near to the " + proximityObjectName + "?");
                interactionButtons.SetActive(true);
                backButton.SetActive(false);
                handMenuManager.HideMenus();
                if (GeneralUIController.Instance.isRecording)
                {
                    handMenuManager.HideRecordButtonInEditMenu();
                }
                _editModeController.CreateAndPublishAction("moves near");
            }

            if (other.CompareTag("Floor"))
            {
                ProximityGameObject1 = other.gameObject;
                string proximityObjectName = ProximityGameObject1.name;
                GeneralUIController.Instance.SetDebugText("Do you want the " + selectedObjBaseName + 
                                                          " to move somewhere near to the " + proximityObjectName + "?");
                interactionButtons.SetActive(true);
                backButton.SetActive(false);
                handMenuManager.HideMenus();
                if (GeneralUIController.Instance.isRecording)
                {
                    handMenuManager.HideRecordButtonInEditMenu();
                }
                _editModeController.CreateAndPublishAction("moves near");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                ChangeMaterial("default");
            }
        }
        
        public void PrepareToRecordMoveAction()
        {
            if (GeneralUIController.Instance.isRecording)
            {
                ECAPrototyping.RuleEngine.Action action = new ECAPrototyping.RuleEngine.Action(selectedObject, "moves near", 
                    ProximityGameObject1);
                GeneralUIController.Instance.InteractionCreationController.SaveRecordedAction(action);
                string screenshotName = selectedObjBaseName + " moves near" + ProximityGameObject1.name;
                _screenshotCamera.SaveImageFromCameraStatic(screenshotCamera.GetComponent<Camera>(), screenshotName);
            }
            GeneralUIController.Instance.EditModeState();
            if (GeneralUIController.Instance.isRecording)
            {
                handMenuManager.HideRecordButtonInEditMenu();
            }
            
            GeneralUIController.Instance.InteractionCreationController.ResetSpawnCubeAndSelectedObject();
        }
        
    
    }

}

