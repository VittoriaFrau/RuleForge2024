using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("text")]
    public class ECAText : MonoBehaviour
    {
        private ObjectsMenuController _objectsMenuController;
        private void Awake()
        {
            var eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
            _objectsMenuController = eventHandler.GetComponent<ObjectsMenuController>();
        }
        
        
        [Action(typeof(ECAText), "changes text")]
        public void ChangeText()
        {
            var keyboard = _objectsMenuController.NonNativeKeyboard.GetComponent<NonNativeKeyboard>();
            GetComponentInChildren<TextMeshPro>().text = keyboard.Text;
        }
        
        [Action(typeof(ECAText), "resets text")]
        public void ResetText()
        {
            this.GetComponentInChildren<TextMeshPro>().text = "Sample example";
        }
    }
}