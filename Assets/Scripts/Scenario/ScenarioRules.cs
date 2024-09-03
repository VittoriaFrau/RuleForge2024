using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRules : MonoBehaviour
{
    private int interactionNumber = 1;
    private GameObject bird, box;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void GoToNextInteraction(){
        this.interactionNumber++;
    }
    
    public void ActivatePlayMode(){
        switch (interactionNumber)
        {
            case 1:
                FindGameObjects();
                //add a on collision enter to the box
                box.AddComponent<ProximityCubeCollision>();
                break;
            
            case 2:

                break;
        }
    }

    public void DeActivatePlayMode()
    {
        box.GetComponent<ProximityCubeCollision>().enabled = false;
        GoToNextInteraction();
    }

    private void FindGameObjects()
    {
        bird = GameObject.FindGameObjectWithTag("Bird");
        box = GameObject.Find("Box");
    }
    
    
    
    
}
