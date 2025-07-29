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
    [RequireComponent(typeof(TextMeshPro))]
    [ECARules4All("counter")]
    public class ECACounter : MonoBehaviour
    {
        [SerializeField] private int counter = 0;
        [SerializeField] private TextMeshPro textMeshPro;

        private void Start()
        {
            textMeshPro = gameObject.GetComponentInChildren<TextMeshPro>();
        }

        [Action(typeof(ECACounter), "increases by one")]
        public void IncreaseCounter()
        {
            counter++;
            textMeshPro.text = "" + counter;
        }

        [Action(typeof(ECACounter), "decreases by one")]
        public void DecreaseCounter()
        {
            counter = counter == 0 ? 0 : --counter;
            textMeshPro.text = "" + counter;
        }
        
        [Action(typeof(ECACounter), "doubles")]
        public void DoubleCounter()
        {
            counter = 2*counter;
            textMeshPro.text = "" + counter;
        }
        
        [Action(typeof(ECACounter), "resets counter")]
        public void ResetCounter()
        {
            counter = 0;
            textMeshPro.text = "0";
        }
    }
}