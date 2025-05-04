using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectsMenuController : MonoBehaviour
{

    public GameObject interactables; //Parent of all interactable gameobjects in the scene
    
    //Getting the prefab of the object to instantiate
    public List<GameObject> shapePrefabs;
    public List<GameObject> animalPrefabs;
    public List<GameObject> furniturePrefabs;
    public List<GameObject> propPrefabs;
    public List<GameObject> environmentPrefabs;
    public List<GameObject> UIPrefabs;
    public List<GameObject> keyboardPrefabs;
    //List of all the objects that have been spawned
    public List<GameObject> spawnedObjects;
        
    private Camera mainCamera;
    
    private Dictionary<string, List<GameObject>> prefabLibrary;
    

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        prefabLibrary = new Dictionary<string, List<GameObject>>()
        {
            {"Shape", shapePrefabs},
            {"Animal", animalPrefabs},
            {"Furniture", furniturePrefabs},
            {"Prop", propPrefabs},
            {"Environment", environmentPrefabs},
            {"UIElement", UIPrefabs}
        };
    }
 

    public void NewShape(string type)
    {
        Utils.InstantiateObject(type, shapePrefabs, mainCamera, interactables.transform);
    }

    public void NewAnimal(string type)
    {
        Utils.InstantiateObject(type, animalPrefabs, mainCamera, interactables.transform);
    }
    
    public void NewFurniture(string type)
    {
        Utils.InstantiateObject(type, furniturePrefabs, mainCamera, interactables.transform);
    }
    
    public void NewProp(string type)
    {
        Utils.InstantiateObject(type, propPrefabs, mainCamera, interactables.transform);
    }
    
    public void NewVegetation(string type)
    {
        Utils.InstantiateObject(type, environmentPrefabs, mainCamera, interactables.transform);
    }
    
    public void NewUIElement(string type)
    {
        GameObject uiElement = Utils.InstantiateObject(type, UIPrefabs, mainCamera, interactables.transform);
        uiElement.GetComponent<Rigidbody>().useGravity = false;
        uiElement.GetComponent<Rigidbody>().isKinematic = true;
    }
    
    public void Spawn(string type, Vector3 position, string prefabKey)
    {
        List<GameObject> prefabs = prefabLibrary[prefabKey];
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, prefabs, mainCamera, interactables.transform, position));
    }
    
    public GameObject NewKeyboard(string type)
    {
        GameObject uiElement = Utils.InstantiateObject(type, keyboardPrefabs, mainCamera, interactables.transform);
        uiElement.GetComponent<Rigidbody>().useGravity = false;
        uiElement.GetComponent<Rigidbody>().isKinematic = true;
        uiElement.GetComponent<BoxCollider>().enabled = false;
        uiElement.transform.position = new Vector3((float)-0.116, (float)1.575, (float)0.717);
        
        Transform[] children = uiElement.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == "NonNativeKeyboard")
            {
                t.gameObject.SetActive(true);
                break;
            }
        }
        return uiElement;
    }
    
    
}
