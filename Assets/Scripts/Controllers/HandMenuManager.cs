using System.Collections;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.UX.Experimental;

namespace UI.RuleEditor
{
    public class HandMenuManager : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject newObjectMenu;
        public GameObject editObjectMenu;
        public GameObject newInteractionMenu;
        public GameObject chooseAnObjectMenu;

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
        
        public GameObject NonNativeKeyboard;
        public GameObject NonNativeNumericKeyboard;
        private int counter = 0;
        private int nElements;
        private int nSeconds;
        
        public GameObject colorPalette;
        private List<GameObject> menus;
        public GameObject debugPanel;
        private Camera _mainCamera;
        private GameObject eventHandler;
        private GeneralUIController generalUIController;
        public GameObject menuContentCanvas;
        // Manual flag to detect the use of Oculus Link / Air Link during tests in the Editor.
        // While running in Play mode inside the Editor, `Application.platform` always returns WindowsEditor,
        // and the XRDisplaySubsystem is not immediately "running" in Start(), making it difficult to
        // determine when the headset is actually active.
        public bool isUsingOculusLink; 
        
        void Start()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            menus = new List<GameObject> {mainMenu, newObjectMenu, editObjectMenu, newInteractionMenu, 
                shapesMenu, colorPalette, chooseAnObjectMenu};
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            generalUIController = eventHandler.GetComponent<GeneralUIController>();
            NonNativeNumericKeyboard.SetActive(true);

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
            }
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
            generalUIController.DeActivatePreviousState(GeneralUIController.UIState.Default);
            debugPanel.SetActive(true);
            mainMenu.SetActive(true);
        }

        public void ShowEditMenu()
        {
            if (generalUIController.GetSelectedObject() != null)
                CustomizeEditModeMenu();
            else chooseAnObjectMenu.SetActive(true);
        }
        
        public void CustomizeEditModeMenu()
        { 
            // Hide all the buttons
            HideEditMenuButtons();
            
            // Show the edit menu
            string objectCategory = Utils.GetECALastScriptFromECAObject(generalUIController.GetSelectedObject());
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
            }
        }

        public void HideEditMenuButtons()
        {
            ecaObjectButtons.ForEach(button => button.SetActive(false));
            ecaCounterButtons.ForEach(button => button.SetActive(false));
            ecaDoorButtons.ForEach(button => button.SetActive(false));
            ecaTextButtons.ForEach(button => button.SetActive(false));
            ecaPropsButtons.ForEach(button => button.SetActive(false));
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
        
        public void ShowKeyboard(string type)
        {
            switch (type)
            {
                case "Keyboard":
                    NonNativeKeyboard.SetActive(true);
                    generalUIController.SetDebugText("Please, type the text you want to insert in the text box.");
                    break;
                case "NumericKeyboard":
                    string name = generalUIController.GetSelectedObject().name;
                    string baseName = name.Substring(0, name.Length - 1);
                    NonNativeNumericKeyboard.SetActive(true);
                    generalUIController.SetDebugText("Please, type the number of " + baseName.ToLower() + "s" +
                                                     " you want to duplicate.\n Press Enter to submit.\n");
                    break;
            }
        }
        
        private void GetTimeIntervalFromKeyboard()
        {
            StartCoroutine(ReEnableKeyboardAfterDelay(0.5f));
        }

        private IEnumerator ReEnableKeyboardAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            NonNativeNumericKeyboard.SetActive(true);
            generalUIController.SetDebugText("Please, type the interval time (seconds) for creating duplicates.\n" +
                                             "Press Enter to submit.");
        }

        public void OnKeyboardSubmit(string receivedText)
        {
            if (counter == 0)
            {
                nElements = int.Parse(receivedText);
                StartCoroutine(ReEnableKeyboardAfterDelay(0.5f));
                counter++;
            }
            else if (counter == 1)
            {
                nSeconds = int.Parse(receivedText);
                NonNativeNumericKeyboard.SetActive(false);
                generalUIController.SetDebugText("N.Elements: " + nElements + "\n N.Seconds: " + nSeconds);
                counter = 0; 
            }
        }


    }

}