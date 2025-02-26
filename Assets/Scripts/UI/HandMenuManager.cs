using MixedReality.Toolkit.SpatialManipulation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.RuleEditor
{
    public class HandMenuManager : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject newObjectMenu;
        public GameObject editObjectMenu;
        public GameObject newInteractionMenu;
        public GameObject chooseAnObjectMenu;
        public GameObject rulePlateMenu;
        public GameObject shapesMenu;
        public GameObject animalMenu;
        public GameObject furnitureMenu;
        public GameObject colorPalette;
        private List<GameObject> menus;
        public GameObject debugPanel;
        private Camera _mainCamera;
        private GameObject eventHandler;
        private GeneralUIController generalUIController;
        public GameObject menuContentCanvas;
        
        void Start()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            menus = new List<GameObject> {mainMenu, newObjectMenu, editObjectMenu, newInteractionMenu, 
                shapesMenu, colorPalette, chooseAnObjectMenu};
            eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            generalUIController = eventHandler.GetComponent<GeneralUIController>();

            //if in unity editor, move the menu closer to the camera
            #if UNITY_EDITOR
                GetComponent<HandConstraintPalmUp>().enabled = false;
                Vector3 forwardOffset = _mainCamera.transform.forward * 0.7f; // 0.5 unità in avanti
                Vector3 leftOffset = -_mainCamera.transform.right * 0.1f; // 0.1 unità a sinistra
                transform.position = _mainCamera.transform.position + forwardOffset + leftOffset;
            #endif
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

        //TODO use taxonomy to show the correct buttons
        public void ShowEditMenu()
        {
            if(generalUIController.GetSelectedObject() != null)
                editObjectMenu.SetActive(true);
            else chooseAnObjectMenu.SetActive(true);
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
    
    }

}