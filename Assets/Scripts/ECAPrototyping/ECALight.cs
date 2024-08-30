using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// <b>ECALight</b> is the class for the lights.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("light")]
    [RequireComponent(typeof(ECAScene))]
    
    public class ECALight : MonoBehaviour
    {
        private Light _light;
        [StateVariable("intensity", ECARules4AllType.Float)] 
        private float _intensity;
        private Slider light_slider;
        public ECABoolean OnOff = new ECABoolean(ECABoolean.BoolType.OFF);
       
        private void Awake()
        {
            _light = gameObject.GetComponent<Light>();
        }
        
        [Action(typeof(ECALight), "changes", "intensity", "to", typeof(float))]
        public void UpdateIntensity(float intensity)
        {
            _intensity = intensity;
            _light.intensity = intensity;
            
        }
        
        /// <summary>
        /// <b>Turn light</b> turns on or off the light of the selected object. 
        /// </summary>
        [Action(typeof(ECALight), "turns", typeof(ECABoolean))]
        public void Turn(ECABoolean mode)
        {
            this.OnOff = mode;
            if (this.OnOff && gameObject.GetComponentInChildren<Light>() != null)
            {
                foreach (Light light in gameObject.GetComponentsInChildren<Light>())
                {
                    light.intensity = 3;
                }
            }
            else if(gameObject.GetComponentInChildren<Light>() != null && !this.OnOff)
            {
                foreach (Light light in gameObject.GetComponentsInChildren<Light>())
                {
                    light.intensity = 0;
                }
            }
            
            //TEST
            if (gameObject.name.Equals("torch")) {
                if (mode)
                {
                    gameObject.GetComponentInChildren<ParticleSystem>().Play();
                }
                else
                {
                    gameObject.GetComponentInChildren<ParticleSystem>().Stop();
                }
            }
                       
        }
         
    }
}