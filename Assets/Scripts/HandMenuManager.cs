using MixedReality.Toolkit.SpatialManipulation;

namespace UI.RuleEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class HandMenuManager : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject newObjectMenu;
        public GameObject editObjectMenu;
        public GameObject newInteractionMenu;
        public GameObject shapesMenu;
        private List<GameObject> menus;
        public List<GameObject> shapePrefabs;
        public List<GameObject> animalPrefabs;
        public List<GameObject> furniturePrefabs;
        private Camera mainCamera;
        public GameObject interactables; //Parent of all interactable gameobjects in the scene

        void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            menus = new List<GameObject> {mainMenu, newObjectMenu, editObjectMenu, newInteractionMenu, shapesMenu};
            ShowMainMenu();
            
            //if in unity editor, move the menu closer to the camera
            #if UNITY_EDITOR
                GetComponent<HandConstraintPalmUp>().enabled = false;
                transform.position = mainCamera.transform.position + mainCamera.transform.forward * 0.5f;
            #endif
        }

        public void ShowMainMenu()
        {
            ShowMenu(mainMenu);
        }
    
        private void ShowMenu(GameObject menu)
        {
            foreach (var m in menus)
            {
                m.SetActive(false);
            }
            menu.SetActive(true);
        }

        public void ShowNewObjectMenu()
        {
            ShowMenu(newObjectMenu);
        }

        public void ShowEditObjectMenu()
        {
            ShowMenu(editObjectMenu);
        }

        public void ShowNewInteractionMenu()
        {
            ShowMenu(newInteractionMenu);
        }

        
    
        public void ShowShapesMenu()
        {
            ShowMenu(shapesMenu);
        }
    
        public void CreateShape(string type)
        {
            UI.Utils.InstantiateObject(type, shapePrefabs, mainCamera, interactables.transform);
        }

        public void CreateAnimal(string type)
        {
            UI.Utils.InstantiateObject(type, animalPrefabs, mainCamera, interactables.transform);
        }
    
        public void CreateFurniture(string type)
        {
            UI.Utils.InstantiateObject(type, furniturePrefabs, mainCamera, interactables.transform);
        }
    
    }

}