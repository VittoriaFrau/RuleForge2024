using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UI.RuleEditor;
using Unity.VisualScripting;
using UnityEngine;

public class ProximityCubeCollision : MonoBehaviour
{
    private Material currentMaterial;
    public Material highlightMaterial;
    private Renderer rend;
    private GameObject eventHandler;
    private GeneralUIController _generalUIController;
    private EditModeController _editModeController;
    private InteractionCreationController _interactionCreationController;
    private GeneralUIController generalUIController;
    private GameObject bird;
    public GameObject screenshotCamera;
    private ScreenshotCamera _screenshotCamera;
    public Texture2D proximityScreenshot;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        currentMaterial = rend.material;
        eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
        _generalUIController = eventHandler.GetComponent<GeneralUIController>();
        bird = GameObject.FindGameObjectWithTag("Bird");
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
                _generalUIController.SetDebugText("The box is colliding with the Bird");
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
