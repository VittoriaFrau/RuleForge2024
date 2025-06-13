using UI;
using UnityEngine;

public class SelectModalityWithHand : MonoBehaviour
{
    private InteractionCreationController _interactionCreationController;
    // This prevents multiple unwanted OnTriggerEnter activations when bubbles redistribute.
    // It adds a temporary lock (isRedistributing) to ignore new trigger events until the redistribution is complete.
    private static bool isRedistributing = false;
    private float distributionTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        var eventHandler = GameObject.FindGameObjectWithTag("EventHandler");
        _interactionCreationController = eventHandler.GetComponent<InteractionCreationController>();
    }
    
    /// <summary>
    /// Controls the collision between the modalities and the hand
    /// </summary>
    /// <param name="other"></param> refears to the hand AttachTransform component
    private void OnTriggerEnter(Collider other)
    {
        if (isRedistributing) return;

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
                case "GameController":
                    _interactionCreationController.SelectModality("Controller");
                    break;
            }
            StartRedistribution(); // Prevents new triggers until redistribution ends
        }
    }
    
    // This function starts the redistribution process and temporarily disables triggers
    public void StartRedistribution()
    {
        isRedistributing = true;
        Invoke(nameof(EndRedistribution), distributionTime); // Waits for 2s before allowing new triggers
    }

    // This function re-enables triggers after redistribution is complete
    private void EndRedistribution()
    {
        isRedistributing = false;
    }
    
}