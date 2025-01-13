using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// A <b>ECAEnvironment</b> is a type of ECAObject, it represents an environment object.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("environment")]
    [RequireComponent(typeof(ECAObject))]

    public class ECAEnvironment : MonoBehaviour
    {
        /// <summary>
        /// <b>GameRender</b> is the renderer of the object.
        /// </summary>
        private Renderer[] gameRenderer;
        
        /// <summary>
        /// <b> Color </b> is the color of the object 
        /// </summary>
        [StateVariable("color", ECARules4AllType.Color)] 
        public Color color;
        
        private void Awake()
        {
            gameRenderer = this.gameObject.GetComponents<Renderer>();
            color = gameRenderer[0].material.color;
        }
    }
}