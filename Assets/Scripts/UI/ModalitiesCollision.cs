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
    /// <param name="other"></param> the object that the hand is colliding with
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
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

    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Headgaze":
                _interactionCreationController.HideModalitiesBubble("Headgaze");
                break;
            case "Laser":
                _interactionCreationController.HideModalitiesBubble("Laser");
                break;
            case "Touch":
                _interactionCreationController.HideModalitiesBubble("Touch");
                break;
            case "Speech":
                _interactionCreationController.HideModalitiesBubble("Speech");
                break;
            case "Proximity":
                _interactionCreationController.HideModalitiesBubble("Proximity");
                break;
        }
    }

    public void EnableHandCollider()
    {
        GameObject hand = GameObject.Find("SolverHandler HandJoint Tracker");
        if (hand.GetComponent<BoxCollider>() == null)
        {
            BoxCollider handCollider = hand.gameObject.AddComponent<BoxCollider>();
            handCollider.isTrigger = true;
            handCollider.AddComponent<ModalitiesCollision>();
            handCollider.size = new Vector3((float)0.20, (float)0.20, (float)0.20);
            handCollider.center = Vector3.zero;
        }
    }
}
    