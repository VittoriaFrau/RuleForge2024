using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectsMenuController : MonoBehaviour
{

    public GameObject interactables; //Parent of all interactable gameobjects in the scene
    
    //Getting the prefab of the object to instantiate
    public List<GameObject> shapePrefabs;
    public List<GameObject> animalPrefabs;
    public List<GameObject> furniturePrefabs;
    public List<GameObject> propPrefabs;
    public List<GameObject> vegetationPrefabs;
    public List<GameObject> UIPrefabs;
    //List of all the objects that have been spawned
    public List<GameObject> spawnedObjects;
    
    public GameObject NonNativeKeyboard;
    private Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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
        Utils.InstantiateObject(type, vegetationPrefabs, mainCamera, interactables.transform);
    }
    
    public void NewUIElement(string type)
    {
        Utils.InstantiateObject(type, UIPrefabs, mainCamera, interactables.transform);
    }
    
    public void SpawnShape(string type, Vector3 position)
    {
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, shapePrefabs, mainCamera, interactables.transform, position));
    }
    
    public void SpawnAnimal(string type, Vector3 position)
    {
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, animalPrefabs, mainCamera, interactables.transform, position));
    }
    
    public void SpawnForniture(string type, Vector3 position)
    {
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, furniturePrefabs, mainCamera, interactables.transform, position));
    }
    
    public void SpawnProp(string type, Vector3 position)
    {
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, propPrefabs, mainCamera, interactables.transform, position));
    }
    
    public void SpawnVegetation(string type, Vector3 position)
    {
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, vegetationPrefabs, mainCamera, interactables.transform, position));
    }
    
    public void SpawnUIElement(string type, Vector3 position)
    {
        spawnedObjects.Add(Utils.InstantiateSpawnObject(type, UIPrefabs, mainCamera, interactables.transform, position));
    }
    
    public void ShowKeyboard()
    {
        NonNativeKeyboard.SetActive(true);
    }
}
