using MixedReality.Toolkit.SpatialManipulation;
using System.Collections.Generic;
using UnityEngine;
        
namespace UI.RuleEditor
{
    public class HandMenuManager : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject newObjectMenu;
        public GameObject editObjectMenu;
        public GameObject newInteractionMenu;
        public GameObject rulePlateMenu;
        public GameObject shapesMenu;
        public GameObject animalMenu;
        public GameObject furnitureMenu;
        public GameObject colorPalette;
        private List<GameObject> menus;
        public GameObject debugPanel;
        public List<GameObject> shapePrefabs;
        public List<GameObject> animalPrefabs;
        public List<GameObject> furniturePrefabs;
        private Camera _mainCamera;
        public GameObject interactables; //Parent of all interactable gameobjects in the scene

        void Start()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            menus = new List<GameObject> {mainMenu, newObjectMenu, editObjectMenu, newInteractionMenu, 
                shapesMenu, colorPalette};

            //if in unity editor, move the menu closer to the camera
            #if UNITY_EDITOR
                GetComponent<HandConstraintPalmUp>().enabled = false;
                Vector3 forwardOffset = _mainCamera.transform.forward * 0.7f; // 0.5 unità in avanti
                Vector3 leftOffset = -_mainCamera.transform.right * 0.1f; // 0.1 unità a sinistra

                transform.position = _mainCamera.transform.position + forwardOffset + leftOffset;
                
            #endif
        }

        /// <summary>
        /// <b>ShowMenu</b> shows the correct menu depending on the current state.
        /// </summary>
        public void ShowMenu(GeneralUIController.UIState uiState)
        {
            HideMenus();
            switch (uiState)
            {
                case GeneralUIController.UIState.Default:
                    mainMenu.SetActive(true);
                    break;
                case GeneralUIController.UIState.NewObject:
                    newObjectMenu.SetActive(true);
                    break;
                case GeneralUIController.UIState.EditMode:
                    break;
                case GeneralUIController.UIState.NewRule:
                    newInteractionMenu.SetActive(true);
                    break;
                case GeneralUIController.UIState.RuleComposition:
                    rulePlateMenu.SetActive(true);
                    debugPanel.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// <b>HideMenus</b> hides all the menus.
        /// </summary>
        private void HideMenus()
        {
            foreach (var m in menus)
            {
                m.SetActive(false);
            }
        }
        
        public void ShowMainMenu()
        {
            HideMenus();
            debugPanel.SetActive(true);
            mainMenu.SetActive(true);
        }
        
        public void ShowEditMenu()
        {
            HideMenus();
            editObjectMenu.SetActive(true);
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
    
        public void CreateShape(string type)
        {
            UI.Utils.InstantiateObject(type, shapePrefabs, _mainCamera, interactables.transform);
        }

        public void CreateAnimal(string type)
        {
            UI.Utils.InstantiateObject(type, animalPrefabs, _mainCamera, interactables.transform);
        }
    
        public void CreateFurniture(string type)
        {
            UI.Utils.InstantiateObject(type, furniturePrefabs, _mainCamera, interactables.transform);
        }
    
    }

}