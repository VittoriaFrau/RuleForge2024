using System.Collections;
using System.Collections.Generic;
using UI;
using UI.RuleEditor;
using UnityEngine;

public class ScenarioRules : MonoBehaviour
{
    private int interactionNumber = 1;
    private GameObject bird, box;

    public GameObject handMenu;
    public GameObject ruleEditorPlate, removableBarrier;
    private RuleManager _ruleManager;
    public GameObject modalityRuleCubePrefab;
    public GameObject actionRuleCubePrefab;
    public GameObject actionRuleCubePrefabVariant;
    public GameObject cubePlate;
    public GameObject screenshotCamera;
    private ScreenshotCamera _screenshotCamera;
    private Vector3 initialPositionBird;
    
    // Start is called before the first frame update
    void Start()
    {
        _ruleManager = this.gameObject.GetComponent<RuleManager>();
        if (screenshotCamera != null)
        {
            _screenshotCamera = screenshotCamera.GetComponent<ScreenshotCamera>();
        }
    }

    public void ShowAndRepositionBird()
    {
        bird.transform.position = initialPositionBird;
        bird.SetActive(true);
    }
    
    public void GoToNextInteraction(){
        this.interactionNumber++;
    }
    
    public void ActivatePlayMode(){
        switch (interactionNumber)
        {
            case 1:
                FindGameObjects();
                // get the box collider component of the box and put istrigger true
                box.GetComponent<BoxCollider>().isTrigger = true;
                // reset physics
                Physics.SyncTransforms(); 
                //add a on collision enter to the box
                box.AddComponent<ProximityCubeCollision>();
                break;
            
            case 2:

                break;
        }
    }

    public void AvoidDoubleCubeContainerActivation(CubeContainer cubeContainer)
    {
        if (interactionNumber == 2)
        {
            cubeContainer.enabled = false;
        }
    }

    public void DeActivatePlayMode()
    {
        box.GetComponent<ProximityCubeCollision>().enabled = false;
        ShowAndRepositionBird();
        GoToNextInteraction();
    }

    private void FindGameObjects()
    {
        bird = GameObject.FindGameObjectWithTag("Bird");
        initialPositionBird = bird.transform.position;
        box = GameObject.Find("Box");
    }


    public void ActivateCombineRulesModeProva()
    {
        handMenu.SetActive(false);
        ruleEditorPlate.SetActive(true);
        removableBarrier.SetActive(true);

        List<ECAEvent> _modalityEvents = new List<ECAEvent>();
        List<ECAEvent> _actionEvents= new List<ECAEvent>();
            
        ECAEvent ecaEvent1 = new ECAEvent(GameObject.FindGameObjectWithTag("Bird"), InteractionCreationController.Modalities.Proximity, "collides");

        ecaEvent1.Texture = LoadTextureFromFile("birdcollision");
        
        ECAEvent ecaEvent2 = new ECAEvent(GameObject.FindGameObjectWithTag("Bird"), "hides");

        _modalityEvents.Add(ecaEvent1);
        _actionEvents.Add(ecaEvent2);
        
        Utils.GenerateCubesFromEventList(_modalityEvents, _actionEvents, 
            modalityRuleCubePrefab, actionRuleCubePrefab, actionRuleCubePrefabVariant, cubePlate, new List<ECAEvent>());

        removableBarrier.SetActive(false);
        
        _ruleManager.InitializeVariables();
            
            
    }
    
    public static Texture2D LoadTextureFromFile(string filename)
    {
        // Verifica se il file esiste
        if (!System.IO.File.Exists(filename))
        {
            Debug.LogError("File not found: " + filename);
            return null;
        }

        // Leggi il file in un array di byte
        byte[] fileData = System.IO.File.ReadAllBytes(filename);

        // Crea una nuova Texture2D
        Texture2D texture = new Texture2D(2, 2); // Le dimensioni iniziali non sono importanti, saranno ridimensionate automaticamente

        // Carica l'immagine dai byte nella texture
        if (texture.LoadImage(fileData))
        {
            // Se il caricamento ha avuto successo, restituisce la texture
            return texture;
        }
        else
        {
            // Se il caricamento fallisce, restituisce null
            Debug.LogError("Failed to load texture from file: " + filename);
            return null;
        }
    }

    
}
