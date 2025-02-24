using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UI.RuleEditor;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class ProximityCubeCollision : MonoBehaviour
    {
        private Material currentMaterial;
        public Material highlightMaterial;
        private Renderer rend;
        public GeneralUIController _generalUIController;
        public GameObject screenshotCamera;
        private ScreenshotCamera _screenshotCamera;
        public Texture2D proximityScreenshot;

        private void Start()
        {
            rend = GetComponent<Renderer>();
            currentMaterial = rend.material;
        
            if (screenshotCamera != null)
            {
                _screenshotCamera = screenshotCamera.GetComponent<ScreenshotCamera>();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }

        private void OnCollisionExit(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }

        //TODO
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Bird"))
            {
                if(gameObject.name.Equals("Box"))
                {
                    other.gameObject.SetActive(false);
                }
                else
                {
                    //Change the material with highlight material
                    rend.material = highlightMaterial;
                    _generalUIController.SetDebugText("The Bird is near to the Box");
                    _screenshotCamera.SaveImageFromCameraStatic(screenshotCamera.GetComponent<Camera>(), "birdcollision");
                }
           
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Bird"))
            {
                //Change the material with highlight material
                rend.material = currentMaterial;
            }
        }
    
    }

}
