using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectsMenuController : MonoBehaviour
{
    public GameObject objectsMenu;

    public GameObject interactables; //Parent of all interactable gameobjects in the scene
    
    //Getting the prefab of the object to instantiate
    public List<GameObject> shapePrefabs;
    public List<GameObject> animalPrefabs;
    public List<GameObject> foodPrefabs;
    public List<GameObject> environmentPrefabs;
    public List<GameObject> musicPrefabs;
    public List<GameObject> lightPrefabs;
    public List<GameObject> effectPrefabs;
    public List<GameObject> doorPrefabs;

    public List<GameObject> categoryButtons;
    private GeneralUIController generalUIController;

    private Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        objectsMenu.SetActive(false);
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        generalUIController = this.gameObject.GetComponent<GeneralUIController>();
    }
 

    public void NewShape(string type)
    {
        Utils.InstantiateObject(type, shapePrefabs, mainCamera, interactables.transform);
    }

    public void NewCharacter(string characterString)
    {
        GameObject character = Utils.InstantiateObject(characterString, animalPrefabs, mainCamera, interactables.transform);
        character.transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public void NewFood(string food)
    {
        Utils.InstantiateObject(food, foodPrefabs, mainCamera, interactables.transform);
    }

    public void NewVegetation(string veg)
    {
        Utils.InstantiateObject(veg, environmentPrefabs, mainCamera, interactables.transform);
    }

    public void NewMusic(string musicString)
    {
        GameObject instrument = Utils.InstantiateObject(musicString, musicPrefabs, mainCamera, interactables.transform);
        switch (musicString)
        {
            case "piano":
                instrument.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            default:
                instrument.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
        }
        
    }

    public void NewLight(string light)
    {
        GameObject lightObject = Utils.InstantiateObject(light, lightPrefabs, mainCamera, interactables.transform);
        switch (light)
        {
            case "lamp":
                lightObject.transform.rotation = Quaternion.Euler(180, 0, 0);
                break;
                
            default:
                lightObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }

    public void NewEffect(string effect)
    {
        GameObject effectGameObject = Utils.InstantiateObject(effect, effectPrefabs, mainCamera, interactables.transform);
        effectGameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void NewDoor(string door)
    {
        GameObject doorGameObject = Utils.InstantiateObject(door, doorPrefabs, mainCamera, interactables.transform);
        doorGameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        doorGameObject.transform.position =
            new Vector3(doorGameObject.transform.position.x, 0f, doorGameObject.transform.position.z);
        GameObject doorChild = doorGameObject.transform.Find("Door").gameObject;
        doorChild.transform.rotation = Quaternion.Euler(-90, 0, 0);
    }
    
    public void DeActivateObjectsMenu()
    {
        foreach (var button in categoryButtons)
        {
            button.SetActive(false);
        }
        objectsMenu.SetActive(false);
    }

    public void ActivateObjectsMenu()
    {
        objectsMenu.SetActive(true);
        generalUIController.NewObjectState();
    }
}
