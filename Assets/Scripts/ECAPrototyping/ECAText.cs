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
        private GameObject keyboard;


        [Action(typeof(ECAText), "changes text")]
        public void ChangeText()
        {
            //todo
        }
        
        [Action(typeof(ECAText), "resets text")]
        public void ResetText()
        {
            this.GetComponentInChildren<TextMeshPro>().text = "Sample example";
        }
    }
}