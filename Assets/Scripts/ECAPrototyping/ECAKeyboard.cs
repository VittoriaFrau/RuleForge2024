using System;
using MixedReality.Toolkit.UX.Experimental;
using UI;
using UnityEngine;


namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("keyboards")]
    
    public class ECAKeyboard:MonoBehaviour
    {
        public string inputText = "";

        public void OnKeyboardInput(string keyboard)
        {
            inputText = GetComponentInChildren<NonNativeKeyboard>().Text;
        }
    }
}