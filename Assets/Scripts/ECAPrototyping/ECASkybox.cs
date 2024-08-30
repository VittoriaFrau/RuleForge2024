using System;
using System.Collections;
using System.Security.Cryptography;
using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    /// <summary>
    /// <b>ECASkybox</b> is the class for the skyboxes.
    /// </summary>
    [DisallowMultipleComponent]
    [ECARules4All("skybox")]
    [RequireComponent(typeof(ECAScene))]
    
    public class ECASkybox : MonoBehaviour
    {

        /// <summary>
        /// <b>Renderer</b> is the material of the skybox.
        /// </summary>
        new Material renderer; 
        private void Awake()
        {
            renderer = RenderSettings.skybox;

        }
        
        // <summary>
        /// <b>SkyboxMaterial</b> sets the material of the skybox, defined by a parameter.
        /// </summary>
        /// <param name="material">The new material to set for the skybox. </param>
        [Action(typeof(ECASkybox), "changes skybox", typeof(string))]
        public void SkyboxMaterial(String skybox)
        {
            switch (skybox)
            {
                case "Day":
                    RenderSettings.skybox = Resources.Load<Material>("Skyboxes/Material/Skybox_Day");
                    break;
                case "Sunset":
                    RenderSettings.skybox = Resources.Load<Material>("Skyboxes/Material/Skybox_Sunset");
                    break;
                case "Night":
                    RenderSettings.skybox = Resources.Load<Material>("Skyboxes/Material/Skybox_Night");
                    break;
                case "Storm":
                    RenderSettings.skybox = Resources.Load<Material>("Skyboxes/Material/DarkStorm");
                    break;
            }
        }
        
    }
}