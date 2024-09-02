using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class ProximityCubeCollision : MonoBehaviour
{
    private Material currentMaterial;
    public Material highlightMaterial;
    private Renderer rend;
    private GameObject eventHandler;
    private GeneralUIController _generalUIController;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        currentMaterial = rend.material;
        eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
        _generalUIController = eventHandler.GetComponent<GeneralUIController>();
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
            //Change the material with highlight material
            rend.material = highlightMaterial;
            _generalUIController.SetDebugText("The box is colliding with the Bird");
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
