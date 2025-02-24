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
        private GeneralUIController _generalUIController;
        private GameObject eventHandler;
        
        private void Start()
        {
            eventHandler = GameObject.Find("EventHandler");
            _generalUIController = eventHandler.GetComponent<GeneralUIController>();
        }
        
        public void ShowEditMenu()
        {
            _generalUIController.SetSelectedObject(gameObject);
            _generalUIController.EditModeState();
        }
        

    }
}