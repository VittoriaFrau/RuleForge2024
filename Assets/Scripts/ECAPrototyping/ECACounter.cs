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
    [ECARules4All("counter")]
    public class ECACounter : MonoBehaviour
    {
        [SerializeField] private int counter = 0;
        
        
        [Action(typeof(ECACounter), "increases by one")]
        public void IncreaseCounter()
        {
            if (gameObject.GetComponentInChildren<TextMeshPro>())
            {
                counter++;
                var text = gameObject.GetComponentInChildren<TextMeshPro>(); 
                text.text = "" + counter;
            }
            else Debug.LogError("The object does not have a TextMeshPro component.");
        }

        [Action(typeof(ECACounter), "decreases by one")]
        public void DecreaseCounter()
        {
            if (gameObject.GetComponentInChildren<TextMeshPro>())
            {
                counter = counter == 0 ? 0 : --counter;
                var text = gameObject.GetComponentInChildren<TextMeshPro>(); 
                text.text = "" + counter;
            }
            else Debug.LogError("The object does not have a TextMeshPro component.");
        }
        
        [Action(typeof(ECACounter), "doubles")]
        public void DoubleCounter()
        {
            if (gameObject.GetComponentInChildren<TextMeshPro>())
            {
                counter = 2*counter;
                var text = gameObject.GetComponentInChildren<TextMeshPro>(); 
                text.text = "" + counter;
            }
            else Debug.LogError("The object does not have a TextMeshPro component.");
        }
        
        [Action(typeof(ECACounter), "resets counter")]
        public void ResetCounter()
        {
            if (gameObject.GetComponentInChildren<TextMeshPro>())
            {
                counter = 0;
                var text = gameObject.GetComponentInChildren<TextMeshPro>(); 
                text.text = "0";
            }
            else Debug.LogError("The object does not have a TextMeshPro component.");
        }
    }
}