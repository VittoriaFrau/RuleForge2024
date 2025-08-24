using System.Collections.Generic;
using MixedReality.Toolkit.SpatialManipulation;
using UI;
using UnityEngine;

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
        GameObject prop = Utils.InstantiateObject(type, propPrefabs, mainCamera, interactables.transform);
        //DEMO: adjust position and rotation of the prop
        if (prop.name.Contains("Belt"))
        {
            prop.transform.localPosition = new Vector3(0, 0.8f, 0);
            prop.transform.localRotation = Quaternion.identity;
            
        }
    }
    
    public void NewVegetation(string type)
    {
        Utils.InstantiateObject(type, environmentPrefabs, mainCamera, interactables.transform);
    }

    public void NewLight(string type)
    {
        GameObject lightGameObject = new GameObject("Light1");
        Light lightComp = lightGameObject.AddComponent<Light>();

        // Configure the light
        lightComp.type = LightType.Point; // Options: Point, Directional, Spot, Area
        lightComp.color = Color.white;
        lightComp.intensity = 1f;
        lightComp.range = 10f;
        // Set parent
        lightGameObject.transform.parent = interactables.transform;
        lightGameObject.tag = "Interactable";
        
        //Add collider
        lightGameObject.AddComponent<SphereCollider>();
        
    }
    
    public GameObject CreateUIElement(string type)
    {
        GameObject uiElement = Utils.InstantiateObject(type, UIPrefabs, mainCamera, interactables.transform);
        uiElement.GetComponent<Rigidbody>().useGravity = false;
        uiElement.GetComponent<Rigidbody>().isKinematic = true;
        Vector3 localPosition = uiElement.transform.localPosition;
        uiElement.transform.localPosition = new Vector3(localPosition.x, 1.8f, localPosition.z);
        return uiElement;
    }

    public void NewUIElement(string type)
    {
        CreateUIElement(type);
    }
    
    public GameObject Spawn(string type, Vector3 position, string prefabKey)
    {
        List<GameObject> prefabs = prefabLibrary[prefabKey];
        GameObject newObj = Utils.InstantiateSpawnObject(type, prefabs, mainCamera, interactables.transform, position);
        spawnedObjects.Add(newObj);
        //DEMO rotate mole
        if (newObj.name.Contains("Mole"))
        {
            newObj.transform.localRotation = 
                Quaternion.Euler(-90f, -180f, 0f);
        }
        if (newObj.name.ToLower().Contains("star"))
        {
            newObj.transform.localRotation = 
                Quaternion.Euler(-90f, 0f, 0f);
            newObj.transform.localPosition = new Vector3(-0.6f, newObj.transform.localPosition.y, newObj.transform.localPosition.z);
        }
        return newObj;
    }
    
    public GameObject NewKeyboard(string type)
    {
        GameObject uiElement = Utils.InstantiateObject(type, keyboardPrefabs, mainCamera, interactables.transform);
        uiElement.GetComponent<Rigidbody>().useGravity = false;
        uiElement.GetComponent<Rigidbody>().isKinematic = true;
        uiElement.GetComponent<BoxCollider>().enabled = false;
        uiElement.transform.localPosition = new Vector3((float)-0.116, (float)1.575, (float)0.717);
        
        // Get the first child (NonNativeKeyboard) and set it active
        var nonNativeKeyboard = uiElement.transform.GetChild(0).gameObject;
        nonNativeKeyboard.SetActive(true);
        nonNativeKeyboard.transform.GetChild(0).gameObject.SetActive(true);
        //DEMO comfortable position
        Destroy(nonNativeKeyboard.GetComponent<RadialView>());
        Destroy(nonNativeKeyboard.GetComponent<SolverHandler>());
        nonNativeKeyboard.transform.localPosition = new Vector3(nonNativeKeyboard.transform.localPosition.x, 0, 3.26f);
        //nonNativeKeyboard.AddComponent<SolverHandler>();
        return uiElement;
    }
    
    
}
