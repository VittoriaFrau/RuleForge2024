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
using UI.RuleEditor;
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
        private HandMenuManager _handMenuManager;

        private void Awake()
        {
            _handMenuManager = GameObject.FindWithTag("HandMenu").GetComponent<HandMenuManager>();
        }
        
        
        [Action(typeof(ECAText), "changes text")]
        public void ChangeText()
        {
            var keyboard = _handMenuManager.NonNativeKeyboard.GetComponent<NonNativeKeyboard>();
            GetComponentInChildren<TextMeshPro>().text = keyboard.Text;
        }
        
        [Action(typeof(ECAText), "resets text")]
        public void ResetText()
        {
            this.GetComponentInChildren<TextMeshPro>().text = "Sample example";
        }
    }
}