using System;
using System.Collections;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ECAPrototyping.RuleEngine;
using UnityEngine.Serialization;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using TMPro;
using Action = ECAPrototyping.RuleEngine.Action;

namespace UI.RuleEditor
{
    public class HandMenuManager : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject newObjectMenu;
        public GameObject editObjectMenu;
        public GameObject newInteractionMenu;
        public GameObject chooseAnObjectMenu;
        public GameObject playMenu;

        public GameObject shapesMenu;
        public GameObject animalMenu;
        public GameObject furnitureMenu;
        public GameObject propsMenu;
        public GameObject vegetationMenu;
		public GameObject uiMenu;

        public List<GameObject> ecaObjectButtons;
        public List<GameObject> ecaCounterButtons;
        public List<GameObject> ecaDoorButtons;
        public List<GameObject> ecaTextButtons;
        public List<GameObject> ecaPropsButtons;
        public List<GameObject> ecaTimerButtons;
        
        private string inputText = "";
        
        public GameObject colorPalette;
        private List<GameObject> menus;
        public GameObject debugPanel;
        private Camera _mainCamera;
        private GameObject eventHandler;
        private ObjectsMenuController objectsMenuController;
        public EditModeController editModeController;
        public GameObject menuContentCanvas;
        // Manual flag to detect the use of Oculus Link / Air Link during tests in the Editor.
        // While running in Play mode inside the Editor, `Application.platform` always returns WindowsEditor,
        // and the XRDisplaySubsystem is not immediately "running" in Start(), making it difficult to
        // determine when the headset is actually active.
        public bool isUsingOculusLink;
        
        public enum KeyboardMode
        {
            EditText,
            SetTimerSeconds,
            SetDuplicateCount
        }

        private void Awake()
        {
            menus = new List<GameObject> {mainMenu, newObjectMenu, editObjectMenu, newInteractionMenu, 
                shapesMenu, colorPalette, chooseAnObjectMenu};
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            objectsMenuController = eventHandler.GetComponent<ObjectsMenuController>();
        }

        void Start()
        {
            //if in unity editor, move the menu closer to the camera
            if (!isUsingOculusLink)
            {
                GetComponent<HandConstraintPalmUp>().enabled = false;
                Vector3 forwardOffset = _mainCamera.transform.forward * 0.7f; // 0.5 unità in avanti
                Vector3 leftOffset = -_mainCamera.transform.right * 0.1f; // 0.1 unità a sinistra
                transform.position = _mainCamera.transform.position + forwardOffset + leftOffset;
            }
            
        }

        /// <summary>
        /// <b>HandleHandMenu</b> shows the correct menu depending on the current state.
        /// </summary>
        public void HandleHandMenu(GeneralUIController.UIState uiState)
        {
            HideMenus();
            switch (uiState)
            {
                case GeneralUIController.UIState.Default:
                    ShowMainMenu();
                    break;
                case GeneralUIController.UIState.NewObject:
                    ShowNewObjectMenu();
                    break;
                case GeneralUIController.UIState.EditMode:
                    ShowEditMenu();
                    break;
                case GeneralUIController.UIState.NewInteraction:
                    ShowNewInteractionMenu();
                    break;
                case GeneralUIController.UIState.RuleComposition:
                    ShowRuleCompositionMenu();
                    break;
                case GeneralUIController.UIState.Play:
                    mainMenu.SetActive(false);
                    debugPanel.SetActive(false);
                    playMenu.SetActive(true);
                    break;
            }
        }

        public void DeActivatePlayStateMenu()
        {
            playMenu.SetActive(false);
            mainMenu.SetActive(true);
            debugPanel.SetActive(true);
        }
        
        /// <summary>
        /// <b>HideMenus</b> hides all the menus.
        /// </summary>
        public void HideMenus()
        {
            foreach (var m in menus)
            {
                m.SetActive(false);
            }
        }

        public void ChangeMenuVisibility(bool visibility)
        {
            menuContentCanvas.SetActive(visibility);
        }
        
        public void ShowMainMenu()
        {
            GeneralUIController.Instance.DeActivatePreviousState(GeneralUIController.UIState.Default);
            debugPanel.SetActive(true);
            mainMenu.SetActive(true);
            GeneralUIController.Instance.SetDebugText("Choose if you want to create an object, modify an existing one or create a rule");
        }

        public void ShowEditMenu()
        {
            if (GeneralUIController.Instance.GetSelectedObject() != null)
                CustomizeEditModeMenu();
            else chooseAnObjectMenu.SetActive(true);
        }
        
        public void CustomizeEditModeMenu()
        { 
            // Hide all the buttons
            HideEditMenuButtons();
            
            // Show the edit menu
            string objectCategory = Utils.GetECALastScriptFromECAObject(GeneralUIController.Instance.GetSelectedObject());
            editObjectMenu.SetActive(true);
            ecaObjectButtons.ForEach(button => button.SetActive(true));
            switch (objectCategory)
            {
                case "Text":
                    ecaTextButtons.ForEach(button => button.SetActive(true));
                    break;
                case "Counter":
                    ecaCounterButtons.ForEach(button => button.SetActive(true));
                    break;
                case "Door":
                    ecaDoorButtons.ForEach(button => button.SetActive(true));
                    break;
                case "Props":
                    ecaPropsButtons.ForEach(button => button.SetActive(true));
                    break;
                case "Timer":
                    ecaTimerButtons.ForEach(button => button.SetActive(true));
                    break;
            }

            if (GeneralUIController.Instance.isRecording)
            {
                HideRecordButtonInEditMenu();
            } 
        }

        public void HideRecordButtonInEditMenu()
        {
            foreach (var button in editObjectMenu.GetComponentsInChildren<PressableButton>(false))
            {
                if (button.gameObject.name.Equals("Record")) button.gameObject.SetActive(false);
            }
        }

        public void HideEditMenuButtons()
        {
            ecaObjectButtons.ForEach(button => button.SetActive(false));
            ecaCounterButtons.ForEach(button => button.SetActive(false));
            ecaDoorButtons.ForEach(button => button.SetActive(false));
            ecaTextButtons.ForEach(button => button.SetActive(false));
            ecaPropsButtons.ForEach(button => button.SetActive(false));
            ecaTimerButtons.ForEach(button => button.SetActive(false));
        }
        
        public void ShowNewInteractionMenu()
        {
            newInteractionMenu.SetActive(true);
        }
        
        public void ShowNewObjectMenu()
        {
            newObjectMenu.SetActive(true);
        }
        
        public void ShowRuleCompositionMenu()
        {
            debugPanel.SetActive(false);
            menuContentCanvas.SetActive(false);
        }
        
        public void ShowShapesMenu()
        {
            HideMenus();
            shapesMenu.SetActive(true);
        }
        
        public void ShowAnimalMenu()
        {
            HideMenus();
            animalMenu.SetActive(true);
        }
        
        public void ShowFurnitureMenu()
        {
            HideMenus();
            furnitureMenu.SetActive(true);
        }
        
        public void ShowPropsMenu()
        {
            HideMenus();
            propsMenu.SetActive(true);
        }
        
        public void ShowVegetationMenu()
        {
            HideMenus();
            vegetationMenu.SetActive(true);
        }
            
        public void ShowUIMenu()
        {
            HideMenus();
            uiMenu.SetActive(true);
        }
        
        public void ShowKeyboard(string keyboardMode)
        {
            if (string.IsNullOrEmpty(keyboardMode)) return;
            // parse keyboardMode to KeyboardMode enum
            if (!Enum.TryParse(keyboardMode, out KeyboardMode keyboardModeEnum))
            {
                Debug.LogError($"Invalid keyboard mode: {keyboardMode}");
                return;
            }
            editObjectMenu.SetActive(false);
            switch (keyboardModeEnum)
            {
                case KeyboardMode.EditText:
                    GameObject keyboard = objectsMenuController.NewKeyboard("Keyboard");
                    GeneralUIController.Instance.SetDebugText("Please, type the text you want to insert in the text box.");
                    CloseButtonClicked(keyboard);
                    EnterButtonClicked(keyboard, (input) =>
                    {
                        GameObject uiElement = GeneralUIController.Instance.GetSelectedObject();
                        if (uiElement != null)
                        {
                            Action action = new Action(GeneralUIController.Instance.GetSelectedObject(), "changes to", input);
                            editModeController.CreateAndPublishAction( "changes text", action);
                            Destroy(keyboard);
                            GeneralUIController.Instance.EditModeState();
                            if (GeneralUIController.Instance.isRecording)
                            {
                                HideRecordButtonInEditMenu();
                            }
                        }
                    });
                    break;
                case KeyboardMode.SetTimerSeconds:
                    GameObject numericKeyboardSeconds = objectsMenuController.NewKeyboard("NumericPad");
                    CloseButtonClicked(numericKeyboardSeconds);
                    EnterButtonClicked(numericKeyboardSeconds, (input) =>
                    {
                        // Create UI element with text of input
                        int nSeconds = int.Parse(input);
                        ECATimer timer = GeneralUIController.Instance.GetSelectedObject().GetComponent<ECATimer>();
                        if (timer != null)
                        {
                            timer.SetTimerDuration(nSeconds);
                        }
                        else
                        {
                            // If the object does not have a timer, create one
                            timer = GeneralUIController.Instance.GetSelectedObject().AddComponent<ECATimer>();
                            timer.SetTimerDuration(nSeconds);
                        }
                        if (GeneralUIController.Instance.isRecording)
                        {
                            ECAEvent ecaEvent = new ECAEvent(GeneralUIController.Instance.GetSelectedObject(), 
                                InteractionCreationController.Modalities.Timer, "hits " + nSeconds + " seconds", nSeconds,
                                Utils.LoadPNG("Assets/Resources/Modalities/timer.png"), false);
                            if (!GeneralUIController.Instance.recordedEvents.Contains(ecaEvent))
                            {
                                GeneralUIController.Instance.recordedEvents.Add(ecaEvent);
                            }
                        }
                        Destroy(numericKeyboardSeconds.gameObject);
                    });
                    break;

                /*else if (counter == 2)
                {
                    var ecaObject = generalUIController.GetSelectedObject();
                    Renderer[] renderers = ecaObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers) r.material = spawnMaterial; // update material

                    ecaObject.GetComponent<ECAObject>().CreateDuplicates(nElements, nSeconds);

                    generalUIController.SetDebugText("N.Elements: " + nElements + "\n N.Seconds: " + nSeconds);
                    editObjectMenu.SetActive(true);
                    Destroy(numericKeyboard.gameObject);
                    counter = 0;
                }
                break;*/
                case KeyboardMode.SetDuplicateCount:
                    GameObject numericKeyboardDuplicate = objectsMenuController.NewKeyboard("NumericPad");
                    GeneralUIController.Instance.SetDebugText("Please, type the number of duplicates you want to create.");
                    CloseButtonClicked(numericKeyboardDuplicate);
                    EnterButtonClicked(numericKeyboardDuplicate, (input) =>
                    {
                        
                        editModeController.CreateAndPublishAction("duplicates", new Action(GeneralUIController.Instance.GetSelectedObject(),
                            "is duplicated", int.Parse(input)));
                        Destroy(numericKeyboardDuplicate.gameObject);
                        GeneralUIController.Instance.EditModeState();
                        //hide record button if it's recording
                        if (GeneralUIController.Instance.isRecording)
                        {
                            HideRecordButtonInEditMenu();
                        }
                    });
            break;
        }
        }
        
        private void CloseButtonClicked(GameObject keyboard)
        {
            Button closeButton = keyboard
                .GetComponentsInChildren<Button>(true) 
                .FirstOrDefault(b => b.name == "close_button");
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    inputText = "";
                    Destroy(keyboard.gameObject);
                    GeneralUIController.Instance.EditModeState();
                });
            }
        }
        
        private void EnterButtonClicked(GameObject keyboard, Action<string> onEnterCallback)
        {
            var script = keyboard.GetComponent<ECAKeyboard>();
            var enterButton = keyboard
                .GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.name == "Enter_Button" && b.gameObject.activeInHierarchy);
            if (enterButton != null)
            {
                enterButton.onClick.AddListener(() =>
                {
                    string inputText = script.inputText;
                    onEnterCallback?.Invoke(inputText);
                });
            }
            /* switch (keyboard.name)
             {
                case "Keyboard1":
                    if (enterButton != null)
                    {
                        enterButton.onClick.AddListener(() =>
                        {
                            string inputText = script.inputText;
                            onEnterCallback?.Invoke(inputText);

                            GameObject uiElement = generalUIController.GetSelectedObject();
                            if (uiElement != null)
                            {
                                var textMesh = uiElement.GetComponentInChildren<TextMeshPro>();
                                textMesh.text = script.inputText;
                                editObjectMenu.SetActive(true);
                                Destroy(keyboard);
                            }
                        });
                    }
                    break;
                case "NumericPad1":
                    if (enterButton != null)
                    {
                        int valueSelected;
                        enterButton.onClick.AddListener(() =>
                        {
                            valueSelected = int.Parse(script.inputText);
                            onEnterCallback?.Invoke(inputText);
                        });
                    }
                    break;
             }*/
        }
        


    }

}