using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using ECAPrototyping.RuleEngine;
using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.Subsystems;
using UI.RuleEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Action = ECAPrototyping.RuleEngine.Action;

namespace UI
{
    public class InteractionCreationController:MonoBehaviour
    {
        public enum Modalities
        {
            None, 
            Headgaze,
            Touch,
            Laser,
            Microgesture,
            Speech,
            Proximity,
            Controller
        }
            
        private Modalities _modality;
        public List<GameObject> modalitiesBubbles;
        private bool bubblesVisible = true;
        public GameObject recordInteractionButton, stopInteractionButton;
        private RuleEngine _ruleEngine;
        //Opposite action events are used to revert the action when we go back to the previous state
        private List<Action> _oppositeActionEvents = new();
        public GameObject screenshotCamera;
        private ScreenshotCamera _screenshotCamera;
        public HandMenuManager handMenuManager;
        private CategoryController _categoryController;
        [FormerlySerializedAs("interactables")] public GameObject interactablesParent;
            
        //Touch modality attributes
        [Header("Touch Modality")]
        public GameObject OpenXRRightHandController;
        public GameObject OpenXRLeftHandController;
        private GameObject RightHand, LeftHand;
        private Material normalTouchMaterial;
        public Material shiningTouchMaterial;
            
        //Microgesture
            
            
        //Laser modality attributes
        [Header("Laser Modality")]
        private GameObject rightHandLaserPointer, leftHandLaserPointer;
        private LineRenderer rightHandLaserPointerLineRenderer, leftHandLaserPointerLineRenderer;
        private Material normalLaserMaterial;
        public Material shiningLaserMaterial;
            
        //Headgaze modality attributes
        [Header("Headgaze Modality")]
        public GameObject headGazePointer;
        public GameObject gazeInteractor;
        private GameObject headGazePointerInstance;
        public Material normalHeadGazeMaterial;
        public Material shiningHeadGazeMaterial;

        //Speech
        [Header("Speech Modality")]
        public GameObject MRTKSpeech;
        public GameObject microphone;
        private List<string> keywords = new() { "fire", "leviosa", "change", "abracadabra" };
            
        // Proximity
        [Header("Proximity Modality")]
        public GameObject proximityCube;
            
        [Header("Controller Modality")]
        public GameObject controllerPrefabLeft;
        public GameObject controllerPrefabRight;
        public GameObject shiningControllerLeft;
        public GameObject shiningControllerRight;
        public GameObject handPrefabLeft;
        public GameObject handPrefabRight;
        private HandModel handModelLeft;
        private HandModel handModelRight;
        public bool isUsingControllers = false;

        private void Start()
        {
            if(screenshotCamera != null) 
                _screenshotCamera =screenshotCamera.GetComponent<ScreenshotCamera>();
            if(MRTKSpeech.activeSelf) MRTKSpeech.SetActive(false);
            if(microphone.activeSelf) microphone.SetActive(false);
            _ruleEngine = RuleEngine.GetInstance();
            _categoryController = GetComponent<CategoryController>();
            handModelLeft = OpenXRLeftHandController.GetComponent<HandModel>();
            handModelRight = OpenXRRightHandController.GetComponent<HandModel>();

            StartCoroutine(CheckControllersWithDelay(2));
        }
        
        
        public IEnumerator CheckControllersWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isUsingControllers = Utils.AreControllersConnected();
            if (isUsingControllers)
            {
                ActivateControllers();
            }
        }

        public void SelectModality(string modality)
        {
            if(_modality != Modalities.None) DeActivateCurrentModality();
                
            _modality = (Modalities) Enum.Parse(typeof(Modalities), modality);
            ShowModalitiesBubbles();
            GeneralUIController.Instance.SetDebugText("Selected modality: " + _modality 
                                                                            + " use your modality to interact with any object in the scene");
            switch (_modality)
            {
                case Modalities.Headgaze:
                    ActivateHeadGazeModality();
                    break;
                case Modalities.Laser:
                    ActivateLaserModality();
                    break;
                case Modalities.Touch:
                    ActivateTouchModality();
                    break;
                case Modalities.Speech:
                    ActivateSpeechModality();
                    break;
                case Modalities.Proximity:
                    ActivateProximityModality();
                    break;
                case Modalities.Controller:
                    ActivateControllerModality();
                    break;
            }
                
            //Se ho selezionato la modalità e sono in modalità registrazione, devo attivare i listener per registrare
            if (GeneralUIController.Instance.isRecording)
            {
                RecordInteraction();
            }
                
        }

        private void ActivateControllers()
        {
            // Left hand
            ReplaceHandModel(handModelLeft, controllerPrefabLeft.transform);
    
            // Right hand
            ReplaceHandModel(handModelRight, controllerPrefabRight.transform);
            
            //Adjust the hand menu solver handle
            Destroy(handMenuManager.GetComponent<HandConstraintPalmUp>());
            Destroy(handMenuManager.GetComponent<SolverHandler>());

            // to make the menu follow the controller, we put the menu as child of the controller
            handMenuManager.transform.SetParent(OpenXRLeftHandController.transform);
            handMenuManager.transform.position = new Vector3(0.115000002f, 0.00899999961f, -0.0240000002f);
            handMenuManager.transform.rotation = Quaternion.Euler(78.0503616f,151.163528f,139.339493f);

        }
            
        private void ActivateControllerModality()
        {
            // Hides modality bubble
            HideModalityBubble("Controller");

            // Left hand
            ReplaceHandModel(handModelLeft, shiningControllerLeft.transform);
    
            // Right hand
            ReplaceHandModel(handModelRight, shiningControllerRight.transform);
        }

        private void ReplaceHandModel(HandModel handModel, Transform newPrefab)
        {
            if (handModel.Model != null)
            {
                Destroy(handModel.Model.gameObject); // Destroy the old model
            }

            handModel.ModelPrefab = newPrefab;

            // Create manually the new model
            Transform newModel = Instantiate(newPrefab, handModel.ModelParent);
    
            // Connect SelectInput, if any
            if (handModel.SelectInput != null && newModel.TryGetComponent(out ISelectInputVisualizer selectInputVisualizer))
            {
                selectInputVisualizer.SelectInput = handModel.SelectInput;
            }
        }
        
        private void DeActivateControllerModality()
        {
            ActivateControllers();
        }

        private void ActivateProximityModality()
        {
            HideModalityBubble("Proximity");

            //Show proximity cube
            proximityCube.SetActive(true);
                
            // loop to the objects in the interactablesParent
            foreach (var go in interactablesParent.transform.GetComponentsInChildren<ObjectManipulator>())
            {
                //Check if the gameobject has a collider component (can be boxcollider, spherecollider, meshcollider)
                if (go.gameObject.GetComponent<Collider>() != null)
                {
                    go.gameObject.GetComponent<Collider>().isTrigger = true;
                    // block the object in the position otherwise it will fall
                    Rigidbody rb = go.gameObject.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                        
                    Physics.SyncTransforms();
                }
            }
        }
            
        private void DeActivateProximityModality()
        {
            //Hide proximity cube
            proximityCube.SetActive(false);
            // loop to the objects in the interactablesParent
            foreach (var go in interactablesParent.transform.GetComponentsInChildren<ObjectManipulator>())
            {
                if (go.gameObject.name.Contains("Box"))
                {
                    go.gameObject.GetComponent<BoxCollider>().isTrigger = false;
                    Physics.SyncTransforms();
                }
            }
        }
            
        private void ActivateHeadGazeModality()
        {
                
            InstantiateHeadGazePointer();
                
            //Change the material of the gaze pointer everytime the user looks at an object
            gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverEntered.AddListener((GameObject) =>
            {
                headGazePointerInstance.GetComponent<Renderer>().material = shiningHeadGazeMaterial;
            });
                
            gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverExited.AddListener((GameObject) =>
            {
                headGazePointerInstance.GetComponent<Renderer>().material = normalHeadGazeMaterial;
            });
                
            //Disappear the Bubble of the modality
            HideModalityBubble("Headgaze");
        }
            
        public void InstantiateHeadGazePointer()
        {
            //Instantiate the headgaze pointer inside the gaze interactor object
            headGazePointerInstance = Instantiate(headGazePointer, gazeInteractor.transform);
            //Set z axes to 0.33
            headGazePointerInstance.transform.localPosition = new Vector3(0,0,0.33f);            
        }

        private void DeActivateHeadGazeModality()
        {
            // remove all listeners from headgazeInteractor
            gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverEntered.RemoveAllListeners();
            gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverExited.RemoveAllListeners();
            Destroy(headGazePointerInstance);
        }

        public void DeActivateCurrentModality()
        {
            switch (_modality)
            {
                case Modalities.Headgaze:
                    DeActivateHeadGazeModality();
                    break;
                case Modalities.Laser:
                    DeActivateLaserModality();
                    break;
                case Modalities.Touch:
                    DeActivateTouchModality();
                    break;
                case Modalities.Speech:
                    DeActivateSpeechModality();
                    break;
                case Modalities.Proximity:
                    DeActivateProximityModality();
                    break;
                case Modalities.Controller:
                    DeActivateControllerModality();
                    break;
            }
            //Remove the listeners
            foreach (var go in interactablesParent.transform.GetComponentsInChildren<ObjectManipulator>())
            {
                RemoveListener(go);
            }
                
            GeneralUIController.Instance.SetDebugText("Selected modality: " + _modality 
                                                                            + " use your modality to interact with any object in the scene");
            _categoryController.ShowCategoryMenu();
        }
            
        private void ActivateTouchModality()
        {
            //Color of the hands to the shining
            GameObject modelParentRight = OpenXRRightHandController.transform
                .Find("[MRTK RightHand Controller] Model Parent").gameObject;
            GameObject modelParentLeft = OpenXRLeftHandController.transform
                .Find("[MRTK LeftHand Controller] Model Parent").gameObject;
                
            GameObject L_R_Hand = modelParentLeft.transform.Find("openxr_left_hand(Clone)").gameObject;
            GameObject R_R_Hand = modelParentRight.transform.Find("openxr_right_hand(Clone)").gameObject;
                
            LeftHand = L_R_Hand.transform.Find("R_Hand").gameObject;
            RightHand = R_R_Hand.transform.Find("R_Hand").gameObject;
                
            normalTouchMaterial = LeftHand.GetComponent<SkinnedMeshRenderer>().material;
                
            RightHand.GetComponent<SkinnedMeshRenderer>().material = shiningTouchMaterial;
            LeftHand.GetComponent<SkinnedMeshRenderer>().material = shiningTouchMaterial;
                
            //Disappear the Bubble of the modality
            HideModalityBubble("Touch");
                
            //Microgesture listener
            /*
             WsClient.StartSocket();
            }*/
                
        }

        public void ActivateLaserModality()
        {
            rightHandLaserPointer = OpenXRRightHandController.transform.Find("Far Ray").gameObject.transform.Find("BendyRay").gameObject;
            leftHandLaserPointer = OpenXRLeftHandController.transform.Find("Far Ray").gameObject.transform.Find("BendyRay").gameObject;
                
            rightHandLaserPointerLineRenderer = rightHandLaserPointer.GetComponent<LineRenderer>();
            leftHandLaserPointerLineRenderer = leftHandLaserPointer.GetComponent<LineRenderer>();

            normalLaserMaterial = rightHandLaserPointer.GetComponent<LineRenderer>().material;
                
            //Assign the new material to both laser hands
            rightHandLaserPointerLineRenderer.material = shiningLaserMaterial;
            leftHandLaserPointerLineRenderer.material = shiningLaserMaterial;

            //Disappear the Bubble of the modality
            HideModalityBubble("Laser");
        }
            
        public void SetLaserPointLineWidth(float width)
        {
            leftHandLaserPointerLineRenderer.widthMultiplier = width;
            rightHandLaserPointerLineRenderer.widthMultiplier = width;
        }

        private void DeActivateLaserModality()
        {
            //Color of the laser back to the normal
            rightHandLaserPointer.GetComponent<LineRenderer>().material = normalLaserMaterial;
            leftHandLaserPointer.GetComponent<LineRenderer>().material = normalLaserMaterial;
        }
            
        private void DeActivateTouchModality()
        {
            //Color of the hands back to the normal
            RightHand.GetComponent<SkinnedMeshRenderer>().material = normalTouchMaterial;
            LeftHand.GetComponent<SkinnedMeshRenderer>().material = normalTouchMaterial;
            /*
                WsClient.StopSocket();
            */
        }

        public void ActivateSpeechModality()
        {
            microphone.SetActive(true);
            HideModalityBubble("Speech");
            GeneralUIController.Instance.SetDebugText("Speak to the microphone");
                
            // if not in unity editor, start the socket
#if !UNITY_EDITOR
                MRTKSpeech.SetActive(true);
                // Get the first running phrase recognition subsystem.
                var keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();

                // If we found one...
                if (keywordRecognitionSubsystem != null)
                {
                    // Register a keyword and its associated action with the subsystem
                    foreach (var keyword in keywords)
                    {
                        keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword).
                            AddListener(() => { GeneralUIController.Instance.SetDebugText("You said " + keyword); });
                    }
                }
#else 
            Debug.Log("You are not using an headset, you can't use the speech modality");
#endif
                
        }

        public void DeActivateSpeechModality()
        {
            MRTKSpeech.SetActive(false);
            microphone.SetActive(false);
        }
            
        public void HideModalitiesBubbles()
        {
            foreach (var go in modalitiesBubbles)
            {
                go.SetActive(false);
            }
            bubblesVisible = false;
        }
            
        public void HideModalityBubble(string modalityName)
        {
            GameObject go = modalitiesBubbles.FirstOrDefault(obj => obj.name == modalityName);
            go.SetActive(false);
        }
            
        public void ShowModalitiesBubbles()
        {
            // if a modality bubble is already selected, hide it
            if(_modality != Modalities.None)
            {
                ShowBubblesExceptSelectedModality();
            }
            else
            {
                //  We create a separate list of elements to remove, avoiding modification of the original list
                // if we are using the controllers we remove the touch bubble, otherwise we remove the controller bubble
                var toRemove = modalitiesBubbles
                    .Where(go => (isUsingControllers && go.name.Equals("Touch")) || (!isUsingControllers && go.name.Equals("Controller")))
                    .ToList();

                foreach (var go in toRemove)
                {
                    modalitiesBubbles.Remove(go);
                    Destroy(go);
                }

                foreach (var go in modalitiesBubbles)
                {
                    go.SetActive(true);
                }
                
            }
            bubblesVisible = true;
        }
        public void ShowBubblesExceptSelectedModality()
        {
            GameObject go = modalitiesBubbles.FirstOrDefault(obj => obj.name == _modality.ToString());
            foreach (var bubble in modalitiesBubbles)
            {
                if(bubble != go)
                    bubble.SetActive(true);
            }
        }
            
        public void StopRecording()
        {
            var uiController = GeneralUIController.Instance;
            uiController.isRecording = false;
            screenshotCamera.SetActive(false);
            uiController.SetDebugText("Recording stopped.");

            if (uiController.UIstate == GeneralUIController.UIState.NewInteraction)
            {
                DeActivateCurrentModality();
                HideModalitiesBubbles();
                return;
            }

            if (uiController.UIstate == GeneralUIController.UIState.EditMode)
            {
                _categoryController.HideCategoryMenu();
                HandleCategoryActions();
            }
                
            _categoryController.ResetCategory();
        }

        private void HandleCategoryActions()
        {
            switch (_categoryController.categoryObjectSelected)
            {
                case CategoryController.CategoryObjectSelected.SingleObject:
                    ExecuteOppositeActions();
                    break;

                case CategoryController.CategoryObjectSelected.Category:
                case CategoryController.CategoryObjectSelected.AllObjects:
                    var lastEvent = GeneralUIController.Instance.recordedEvents.Last();
                    lastEvent.ChangeObjectCategory(_categoryController.categoryObjectSelected);
            
                    if (_categoryController.categoryObjectSelected == CategoryController.CategoryObjectSelected.Category)
                    {
                        ExecuteActionOnCategory();
                    }
                    else
                    {
                        ExecuteActionOnAllObjects();
                    }
                    break;
            }
        }

        private void ExecuteOppositeActions()
        {
            foreach (var action in _oppositeActionEvents)
            {
                _ruleEngine.ExecuteAction(action);
            }
        }

        private void ExecuteActionOnCategory()
        {
            foreach (var action in _oppositeActionEvents)
            {
                Utils.ExecuteActionOnCategory(_ruleEngine, action, interactablesParent);
            }
        }

        private void ExecuteActionOnAllObjects()
        {
            foreach (var action in _oppositeActionEvents)
            {
                Utils.ExecuteActionOnAllObjects(_ruleEngine, action, interactablesParent);
            }
        }

        public void StartRecording()
        {
            GeneralUIController.Instance.isRecording = true;

            switch (GeneralUIController.Instance.UIstate)
            {
                case GeneralUIController.UIState.NewInteraction:
                    RecordInteraction();
                    break;
                case GeneralUIController.UIState.EditMode:
                    _categoryController.CustomizeCategoryMenu(GeneralUIController.Instance.GetSelectedObject());
                    RecordAction();
                    break;
            }
        }

        public void RecordAction()
        {
            GeneralUIController.Instance.SetDebugText("Recording started.");
            _oppositeActionEvents.Clear();
        }
            
        public void SaveRecordedAction(Action action)
        {
            ECAEvent ecaEvent = Utils.ConvertActionToECAEvent(action);
            GameObject selectedObject = GeneralUIController.Instance.GetSelectedObject();
            if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
            {
                Action oppositeAction = Utils.GetOppositeAction(action, ecaEvent);
                _oppositeActionEvents.Add(oppositeAction);
                GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                Debug.Log("Saved action: " + ecaEvent);
                GeneralUIController.Instance.SetDebugText(ecaEvent.ToString());
                PrepareForActionScreenShot(selectedObject);
            }
        }

        public void RecordInteraction()
        {
            if (_modality == Modalities.Speech)
            {
                GeneralUIController.Instance.SetDebugText("Recording started. Please, speak to the microphone");
            }
            if(_modality == Modalities.Proximity)
            {
                GeneralUIController.Instance.SetDebugText("Recording started. Please, use the proximity cube as trigger");

                /*//check if the proximity cube's isTrigger is true
                if (proximityCube.GetComponentsInChildren<BoxCollider>().FirstOrDefault().isTrigger)
                {
                    //change the material of the proximity cube
                    proximityCube.GetComponentsInChildren<ProximityCubeCollision>().FirstOrDefault()?.ChangeMaterial("record");
                    generalUIController.SetDebugText("Recording started. Please, interact with the proximity cube");
                }
                else
                {
                    generalUIController.SetDebugText("Please, set the proximity cube as trigger");
                    return;
                }*/

            }
            else GeneralUIController.Instance.SetDebugText("Recording started. Please, interact with an object");
                
            if (_modality == Modalities.None)
            {
                Debug.Log("No modality selected");
                GeneralUIController.Instance.SetDebugText("No modality selected, please select one");
                return;
            }
                
            recordInteractionButton.SetActive(false);
            stopInteractionButton.SetActive(true);

            foreach (var go in interactablesParent.transform.GetComponentsInChildren<ObjectManipulator>())
            {
                AddListener(go);
            }
        }

        private void AddListener(ObjectManipulator manipulator)
        {
            switch (_modality)
            {
                case Modalities.Headgaze:
                    AddHeadGazeListener(manipulator);
                    break;
                case Modalities.Laser:
                    AddLaserListener(manipulator);
                    break;
                case Modalities.Touch:
                    AddTouchListener(manipulator);
                    break;
                case Modalities.Speech:
                    AddSpeechListener();
                    break;
                case Modalities.Proximity:
                    AddProximityListener(manipulator);
                    break;
                case Modalities.Controller:
                    AddControllersListener(manipulator);
                    break;
            }
                
        }

        private void RemoveListener(ObjectManipulator manipulator)
        {
            switch (_modality)
            {
                case Modalities.Headgaze:
                    gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverEntered.RemoveAllListeners();
                    gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverExited.RemoveAllListeners();
                    break;
                case Modalities.Laser:
                    manipulator.hoverEntered.RemoveAllListeners();
                    manipulator.hoverExited.RemoveAllListeners();
                    break;
                case Modalities.Touch:
                case Modalities.Controller:
                    manipulator.OnClicked.RemoveAllListeners();
                    manipulator.selectEntered.RemoveAllListeners();
                    manipulator.selectExited.RemoveAllListeners();
                    break;
            }
        }
            
        public void DeActivateNewInteraction()
        {
            _modality = Modalities.None;
            DeActivateCurrentModality();
            HideModalitiesBubbles();
        }

        IEnumerator TakeScreenShot(List<ECAEvent> _events)
        {
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            //Set the texture of the last events in the list to the screenshot
            _events.Last().Texture = texture;
        }
            
        private void AddHeadGazeListener(ObjectManipulator manipulator)
        {
            GameObject gameObject = manipulator.gameObject;

            gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverEntered.AddListener((GameObject) =>
            {
                Debug.Log(gameObject.name + " Hover entered");
                GeneralUIController.Instance.SetDebugText("You are pointing " + manipulator.gameObject.name);

                    
                //Note: event should be added before starting the coroutine
                ECAEvent ecaEvent = new ECAEvent(gameObject, Modalities.Headgaze, "looks", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(gameObject, Modalities.Headgaze, ecaEvent);
                }
                    
                _categoryController.CustomizeCategoryMenu(gameObject);      
            });
                
            /* gazeInteractor.GetComponent<FuzzyGazeInteractor>().hoverExited.AddListener((GameObject) =>
             {
                 Debug.Log(manipulator.gameObject.name + " Hover exited");
                 GeneralUIController.Instance.SetDebugText("You stopped pointing " + manipulator.gameObject.name);
                 ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Headgaze, "stops looking", null, false);
                 if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                 {
                     GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                     PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Headgaze, ecaEvent);
                 }
             });*/
        }
            
        private void AddLaserListener(ObjectManipulator manipulator)
        {
            GameObject gameObject = manipulator.gameObject;
            manipulator.hoverEntered.AddListener(interactor =>
            {
                Debug.Log("Hover entered");
                GeneralUIController.Instance.SetDebugText("You are pointing " + manipulator.gameObject.name);
                    
                //Set the laser pointer line width for the screenshot
                SetLaserPointLineWidth(30.0f);
                    
                //Note: event should be added before starting the coroutine
                    
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Laser, "points", null,false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Laser, ecaEvent);
                }
                _categoryController.CustomizeCategoryMenu(gameObject);
            });

            manipulator.hoverExited.AddListener(interactor =>
            {
                Debug.Log(manipulator.gameObject.name +" Hover exited"); 
                GeneralUIController.Instance.SetDebugText("You stopped pointing " + manipulator.gameObject.name);
                        
                //Note: event should be added before starting the coroutine
                //ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Laser, "Hover Exited");
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Laser, "stops pointing", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Laser, ecaEvent);
                }
            });
        }

        private void AddTouchListener(ObjectManipulator manipulator)
        {
            GameObject gameObject = manipulator.gameObject;

            //attach listener to object manipulator manipulation started event
            manipulator.OnClicked.AddListener (() =>
            {
                Debug.Log(manipulator.gameObject.name + " On clicked");
                GeneralUIController.Instance.SetDebugText("You clicked on " + manipulator.gameObject.name);
                    
                //Note: event should be added before starting the coroutine
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Touch, "Clicks", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Touch, ecaEvent);
                }

                _categoryController.CustomizeCategoryMenu(gameObject);

            });
                
            manipulator.selectEntered.AddListener(interactor =>
            {
                Debug.Log(manipulator.gameObject.name + " Select entered");
                GeneralUIController.Instance.SetDebugText("You selected " + manipulator.gameObject.name);
                    
                _categoryController.CustomizeCategoryMenu(gameObject);
                    
                //Note: event should be added before starting the coroutine
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Touch, "selects", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Touch, ecaEvent);
                }

            });
                    
            manipulator.selectExited.AddListener(interactor =>
            {
                Debug.Log(manipulator.gameObject.name + " Select exited");
                GeneralUIController.Instance.SetDebugText("You deselected " + manipulator.gameObject.name);
                //Note: event should be added before starting the coroutine
                //Add the event only if it doesn't exist already
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Touch, "deselects", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Touch, ecaEvent);
                }
            });
        }

        private void AddControllersListener(ObjectManipulator manipulator)
        {
            GameObject gameObject = manipulator.gameObject;

            //attach listener to object manipulator manipulation started event
            manipulator.OnClicked.AddListener (() =>
            {
                Debug.Log(manipulator.gameObject.name + " On clicked");
                GeneralUIController.Instance.SetDebugText("You clicked on " + manipulator.gameObject.name);
                    
                //Note: event should be added before starting the coroutine
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Controller, "Clicks", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Controller, ecaEvent);
                }

                _categoryController.CustomizeCategoryMenu(gameObject);

            });
                
            manipulator.selectEntered.AddListener(interactor =>
            {
                Debug.Log(manipulator.gameObject.name + " Select entered");
                GeneralUIController.Instance.SetDebugText("You selected " + manipulator.gameObject.name);
                    
                _categoryController.CustomizeCategoryMenu(gameObject);
                    
                //Note: event should be added before starting the coroutine
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Controller, "selects", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Controller, ecaEvent);
                }

            });
                    
            manipulator.selectExited.AddListener(interactor =>
            {
                Debug.Log(manipulator.gameObject.name + " Select exited");
                GeneralUIController.Instance.SetDebugText("You deselected " + manipulator.gameObject.name);
                //Note: event should be added before starting the coroutine
                //Add the event only if it doesn't exist already
                ECAEvent ecaEvent = new ECAEvent(manipulator.gameObject, Modalities.Controller, "deselects", null, false);
                if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                {
                    GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                    PrepareForModalityScreenshot(manipulator.gameObject, Modalities.Controller, ecaEvent);
                }
            });
        }
            

        private void AddSpeechListener()
        {
            // Get the first running phrase recognition subsystem.
            var keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();

            // If we found one...
            if (keywordRecognitionSubsystem != null)
            {
                // Register a keyword and its associated action with the subsystem
                foreach (var keyword in keywords)
                {
                    keywordRecognitionSubsystem.CreateOrGetEventForKeyword(keyword).
                        AddListener(() =>
                        {
                            GeneralUIController.Instance.SetDebugText("You said " + keyword);
                            if (GeneralUIController.Instance.isRecording)
                            {
                                //Generate ecaevent
                                ECAEvent ecaEvent = new ECAEvent(null, Modalities.Speech, keyword, 
                                    Utils.LoadPNG("Assets/Resources/Icons/microphone.png"), false);
                                if(!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent)) GeneralUIController.Instance.recordedEvents.Add(ecaEvent);    
                            }
                        });
                        
                }
            }
        }

        private void AddProximityListener(ObjectManipulator manipulator)
        {
            proximityCube.GetComponentInChildren<ObjectManipulator>().enabled = false;
                
            foreach (var go in interactablesParent.transform.GetComponentsInChildren<ObjectManipulator>())
            {
                //Check if the gameobject has a collider component (can be boxcollider, spherecollider, meshcollider)
                if (go.gameObject.GetComponent<Collider>() != null)
                {
                    go.gameObject.GetComponent<Collider>().isTrigger = false;
                    // block the object in the position otherwise it will fall
                    Rigidbody rb = go.gameObject.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.constraints = RigidbodyConstraints.None;
                        
                    Physics.SyncTransforms();
                }
            }
        }

        public void CreateProximityCube(GameObject proximityGameObject1, GameObject proximityGameObject2)
        {
            ECAEvent ecaEvent = new ECAEvent(proximityGameObject1, Modalities.Proximity, "is near", 
                proximityGameObject2, null, false);
            if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
            {
                GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                PrepareForModalityScreenshot(proximityGameObject1, Modalities.Touch, ecaEvent);
            }
        }
            
            

        public void PrepareForActionScreenShot(GameObject gameObject)
        {
            handMenuManager.ChangeMenuVisibility(false); // makes the handmenu disappear
            _screenshotCamera.TakeActionScreenshot(gameObject, GeneralUIController.Instance.recordedEvents.Last());
            handMenuManager.ChangeMenuVisibility(true); // makes the handmenu reappear
        }
            
        public void PrepareForModalityScreenshot(GameObject gameObject, Modalities modality, ECAEvent ecaEvent)
        {
            if(bubblesVisible) HideModalitiesBubbles();
            handMenuManager.ChangeMenuVisibility(false); // makes the handmenu disappear
            _screenshotCamera.TakeModalityScreenshot(gameObject, modality, ecaEvent);
            if(bubblesVisible) ShowBubblesExceptSelectedModality();
            handMenuManager.ChangeMenuVisibility(true); // makes the handmenu reappear
        }
    }
}