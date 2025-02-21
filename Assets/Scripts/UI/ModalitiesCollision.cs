using System;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalitiesCollision : MonoBehaviour
{
    private InteractionCreationController _interactionCreationController;

    // Start is called before the first frame update
    void Start()
    {
        var eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
        _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    
    /// <summary>
    /// Controls the collision between the modalities and the hand
    /// </summary>
    /// <param name="other"></param> refears to the hand AttachTransform component
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name=="AttachTransform") 
        {
            switch (gameObject.tag)
            {
                case "Headgaze":
                    _interactionCreationController.SelectModality("Headgaze");
                    break;
                case "Laser":
                    _interactionCreationController.SelectModality("Laser");
                    break;
                case "Touch":
                    _interactionCreationController.SelectModality("Touch");
                    break;
                case "Speech":
                    _interactionCreationController.SelectModality("Speech");
                    break;
                case "Proximity":
                    _interactionCreationController.SelectModality("Proximity");
                    break;
            }
        }
    }
    
}