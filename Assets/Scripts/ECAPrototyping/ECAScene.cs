using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// A <b>Scene</b> is a type of ECAObject.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("Scene")]
    [RequireComponent(typeof(ECAObject))]

    public class ECAScene : MonoBehaviour
    {
        /// <summary>
        /// <b>GameRender</b> is the renderer of the object.
        /// </summary>
        private Renderer gameRenderer;
        
        /// <summary>
        /// <b> Color </b> is the color of the object 
        /// </summary>
        [StateVariable("color", ECARules4AllType.Color)] 
        public Color color;
        
        private void Awake()
        {
            gameRenderer = this.gameObject.GetComponent<Renderer>();
            color = gameRenderer.material.color;
        }
    }
}