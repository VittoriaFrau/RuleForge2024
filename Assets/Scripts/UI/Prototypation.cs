using System;
using System.Collections.Generic;
using ECAPrototyping.RuleEngine;
using MixedReality.Toolkit.SpatialManipulation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UI
{
    public class Prototypation: MonoBehaviour
    {
        private EditModeController _editModeController;
        private ObjectManipulator _objectManipulator;
        private GeneralUIController _generalUIController;
        private InteractionCreationController _interactionCreationController;
        private GameObject eventHandler;
        private bool action_executed;
        
        private void Start()
        {
            eventHandler = GameObject.Find("EventHandler");
            _editModeController = eventHandler.GetComponent<EditModeController>();
            _objectManipulator = GetComponent<ObjectManipulator>();
            _generalUIController = eventHandler.GetComponent<GeneralUIController>();
            _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();

            /*if (_editModeController.EditMode)
            {
                //Show the PiMenu when the object is selected
                UnityAction<SelectEnterEventArgs> firstSelectEntered = null;
                firstSelectEntered = args =>
                {
                    _editModeController.ShowHidePiUIMenu(true);
                    _objectManipulator.firstSelectEntered.RemoveListener(firstSelectEntered);
                };
                _objectManipulator.firstSelectEntered.AddListener(firstSelectEntered);
            }*/
        }
        
        /*public void ShowPieUIMenu()
        {
            if (_editModeController.EditMode)
            {
                _editModeController.ShowHideRadialMenu(true);
                _editModeController.SelectedObject = gameObject;
                _generalUIController.SelectedObject(gameObject.name);
                
                if (_generalUIController.UIstate == GeneralUIController.UIState.EditMode)
                {
                    foreach (var button in _editModeController.sceneButtonsToClose)
                    {
                        if(button) button.SetActive(false);
                    }
                    _generalUIController.radialMenu.getListButtons(CheckECAObject(_editModeController.SelectedObject), false, true);
                }
                else _generalUIController.radialMenu.getListButtons(CheckECAObject(_editModeController.SelectedObject));
                _generalUIController.radialMenu.gameObject.SetActive(true);
            }
        }*/
        

        //Function to check if the object is ECA Music/Character (only for the fist time I select the object)
        public List<GameObject> CheckECAObject(GameObject gameObject)
        {
                if (gameObject.GetComponent<ECACharacter>())
                {
                    if (!_generalUIController.editingButtonsTMP.Contains(_generalUIController.characterButtons[0]))
                    {
                        _generalUIController.editingButtonsTMP.AddRange(_generalUIController.characterButtons);
                    }
                }

                if (gameObject.GetComponent<ECAMusic>())
                {
                    if (!_generalUIController.editingButtonsTMP.Contains(_generalUIController.musicButtons[0]))
                    {
                        _generalUIController.editingButtonsTMP.AddRange(_generalUIController.musicButtons);
                    }
                }

                if (gameObject.GetComponent<ECALight>())
                {
                    if (!_generalUIController.editingButtonsTMP.Contains(_generalUIController.lightButtons[0]))
                    {
                        _generalUIController.editingButtonsTMP.AddRange(_generalUIController.lightButtons);
                    }
                }
                
                if (gameObject.GetComponent<ECAEffect>())
                {
                    if (!_generalUIController.editingButtonsTMP.Contains(_generalUIController.effectButtons[0]))
                    {
                        _generalUIController.editingButtonsTMP.AddRange(_generalUIController.effectButtons);
                    }
                }

                if (gameObject.GetComponent<ECADoor>())
                {
                    if (!_generalUIController.editingButtonsTMP.Contains(_generalUIController.doorButtons[0]))
                    {
                        _generalUIController.editingButtonsTMP.AddRange(_generalUIController.doorButtons);
                    }
                }
                
                return _generalUIController.editingButtonsTMP;
        }

        public void HidePieUIMenu()
        {
            if(!_editModeController.EditMode) _editModeController.ShowHideRadialMenu(false);
        }
    }
}