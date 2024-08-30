using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// <b>ECAObject</b> is the base class for all objects that can be used in the rule engine.
    /// All the other classes in this package inherit from this class or one of its subclasses.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("effect")]
    public class ECAEffect : MonoBehaviour
    {
        /// <summary>
        /// <b>GameRender</b> is the renderer of the object.
        /// </summary>
        private Renderer[] gameRenderer;
        
        private ParticleSystem[] effect;
        private void Awake()
        {
            gameRenderer = this.gameObject.GetComponents<Renderer>();
            effect = this.gameObject.GetComponentsInChildren<ParticleSystem>();
        }
        
        [Action(typeof(ECAEffect), "changes intensity", typeof(float))]
        public void UpdateIntensity(float intensity)
        {
            foreach (var particleSystem in effect)
            {
                var main = particleSystem.main;
                main.maxParticles = (int) intensity;
            }
            
        }
    }
}