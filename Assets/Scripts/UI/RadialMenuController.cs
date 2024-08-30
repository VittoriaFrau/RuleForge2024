using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RadialMenuController: MonoBehaviour
    {
        public GameObject optionsMenu, newObjectButtons, shapeButtons, editButtons, colorButtons, interactionButtons;

        public void ShowOptionsMenu()
        {
            //Hide all the other menus
            newObjectButtons.SetActive(false);
            shapeButtons.SetActive(false);
            editButtons.SetActive(false);
            colorButtons.SetActive(false);
            interactionButtons.SetActive(false);
            //Show the options menu
            optionsMenu.SetActive(true);
        }
        
        public void ShowNewObjectButtons()
        {
            //Hide all the other menus
            optionsMenu.SetActive(false);
            shapeButtons.SetActive(false);
            editButtons.SetActive(false);
            colorButtons.SetActive(false);
            interactionButtons.SetActive(false);
            //Show the new object buttons
            newObjectButtons.SetActive(true);
        }
        
        public void ShowShapeButtons()
        {
            //Hide all the other menus
            optionsMenu.SetActive(false);
            newObjectButtons.SetActive(false);
            editButtons.SetActive(false);
            colorButtons.SetActive(false);
            interactionButtons.SetActive(false);
            //Show the shape buttons
            shapeButtons.SetActive(true);
        }
        
        public void ShowEditButtons()
        {
            //Hide all the other menus
            optionsMenu.SetActive(false);
            newObjectButtons.SetActive(false);
            shapeButtons.SetActive(false);
            colorButtons.SetActive(false);
            interactionButtons.SetActive(false);
            //Show the edit buttons
            editButtons.SetActive(true);
        }
        
        public void ShowColorButtons()
        {
            //Hide all the other menus
            optionsMenu.SetActive(false);
            newObjectButtons.SetActive(false);
            shapeButtons.SetActive(false);
            editButtons.SetActive(false);
            interactionButtons.SetActive(false);
            //Show the color buttons
            colorButtons.SetActive(true);
        }
        
        public void ShowInteractionButtons()
        {
            //Hide all the other menus
            optionsMenu.SetActive(false);
            newObjectButtons.SetActive(false);
            shapeButtons.SetActive(false);
            editButtons.SetActive(false);
            colorButtons.SetActive(false);
            //Show the interaction buttons
            interactionButtons.SetActive(true);
        }
        
        public void HideRadialMenu()
        {
            //Hide all the menus
            optionsMenu.SetActive(false);
            newObjectButtons.SetActive(false);
            shapeButtons.SetActive(false);
            editButtons.SetActive(false);
            colorButtons.SetActive(false);
            interactionButtons.SetActive(false);
        }

    }
}