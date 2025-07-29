using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Controllers;
using ECAPrototyping.RuleEngine;
using ECAPrototyping.Utils;
using MixedReality.Toolkit.UX;
//using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UI.RuleEditor;
using Unity.VisualScripting;
using Action = ECAPrototyping.RuleEngine.Action;
using Object = UnityEngine.Object;
using UnityEngine.XR;



namespace UI
{
    public class Utils
    {
        public static List<Transform> FindAllChildrenWithName(Transform parent, string nameToFind)
        {
            var result = new List<Transform>();

            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == nameToFind)
                    result.Add(child);
            }

            return result;
        }

        public static GameObject GetPrefabFromString(string s, List<GameObject> prefabs)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.name == s)
                {
                    return prefab;
                }
            }

            return null;
        }

        public static Color GetColorFromString(string s)
        {
            if (ECAColor.colorDict.ContainsKey(s))
            {
                return ECAColor.colorDict[s];
            }

            return Color.white;
        }

        
        //"providerType":"glove","providerId":42,"timestamp":1687168716976,"name":"tap","actuator":"thumb","contact":"index tip","raw":{}}}}
        /*public static MicrogestureData convertJsonToMicrogestureData(string json)
        {
            dynamic jsonObject = JsonConvert.DeserializeObject(json);
            string providerType = jsonObject.microgesture.content.providerType;
            int providerId = jsonObject.microgesture.content.providerId;
            long timestamp = jsonObject.microgesture.content.timestamp;
            string name = jsonObject.microgesture.content.name;
            string actuator = jsonObject.microgesture.content.actuator;
            string contact = jsonObject.microgesture.content.contact;
            
            MicrogestureData data = new MicrogestureData(providerType, providerId, timestamp, name, actuator, contact);
            return data;
        }*/
        
        public static Texture2D LoadPNG(string filePath) {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath)) 	{
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }

        //Remove duplicate events and events with null texture
        public static List<ECAEvent> RemoveDuplicates(List<ECAEvent> events)
        {
            List<ECAEvent> filteredEvents = new List<ECAEvent>();
            foreach (var e in events)
            {
                if (!filteredEvents.Contains(e) && e.Texture != null)
                {
                    filteredEvents.Add(e);
                }
            }

            return filteredEvents;
        }

        /*public static Dictionary<GameObject, Vector3> GenerateCubesFromEventList(List<ECAEvent> _modalityEvents, List<ECAEvent>
            _actionEvents, GameObject cubePrefab, GameObject prefabVariant, GameObject cubePlate)
        {
            Dictionary<GameObject, Vector3> result = new Dictionary<GameObject, Vector3>();
            //Filter events 
            List<ECAEvent> filteredModalityEvents = RemoveDuplicates(_modalityEvents);

            float previousZ = 1.41f;
            //Generate cubes
            foreach (var e in filteredModalityEvents)
            {
                //Adjust cube transform
                Vector3 position = CalculatePositionInPlate(previousZ, filteredModalityEvents.IndexOf(e));
                previousZ = position.z;

                GameObject cube = InstantiateRuleCube(cubePrefab, 1, position, cubePlate.transform, new Texture[]{e.Texture});
                result.Add(cube, position);
                FillTextLabelsInCube(e, cube);
            }

            List<ECAEvent> filteredActionsEvents = RemoveDuplicates(_actionEvents);
            
            foreach (var e in filteredActionsEvents)
            {
                //Adjust cube transform
                Vector3 position = CalculatePositionInPlate(previousZ, filteredActionsEvents.IndexOf(e));
                previousZ = position.z;
                GameObject cube;
                if (e.Object != "")
                {
                    cube = InstantiateRuleCube(cubePrefab, 1, position, cubePlate.transform, 
                        new Texture[]{e.Texture});
                }
                else
                {
                    cube = InstantiateRuleCube(prefabVariant, 1, position, cubePlate.transform, 
                        new Texture[]{e.Texture});
                }

                cube.tag = "ActionRuleCube";
                
                result.Add(cube, position);
                FillTextLabelsInCube(e, cube);
            }
            
            return result;
        }*/
        
        public static void GenerateCubesFromEventList(List<ECAEvent> recordedEvents, 
            GameObject modalityCubePrefab, GameObject actionCubePrefab, 
            GameObject actionCubePrefabVariant, GameObject cubePlate)
        {

            // Generate cubes for all events
            float previousZ = 1.41f;
            int i = 0;
            foreach (var e in recordedEvents)
            { 
                //Vector3 position = CalculatePositionInPlate(previousZ, allEvents.IndexOf(e));
                //previousZ = position.z;
                Vector3 staticLocalPosition = CalculateStaticLocalPosition(i);
                GameObject cube;
                
                if (!e.IsActionEvent)
                {
                    if (e.Texture == null)
                    {
                        Debug.LogError("Texture is null for event: " + e);
                    }
                    cube = InstantiateRuleCube(modalityCubePrefab, 1, 
                        staticLocalPosition, cubePlate.transform, new Texture[] { e.Texture });
                }
                else cube = InstantiateRuleCube(e.ObjectStr == null ? actionCubePrefabVariant : actionCubePrefab, 1, 
                    staticLocalPosition, cubePlate.transform, new Texture[] { e.Texture });

                e.CubeID = cube.GetComponent<CubeController>().cubeID;
                e.CubeInitialPosition = staticLocalPosition;
                FillTextLabelsInCube(e, cube);
                i++;
            }

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

        private static Vector3 CalculateStaticLocalPosition(int i)
        {
            switch (i)
            {
               case 0:
                   return new Vector3(-5.3f, -77.9f, -16.3f);
               case 1:
                   return new Vector3(-5.3f, -42.5f, -16.3f);
               case 2:
                   return new Vector3(-5.3f, -8.4f, -16.3f);
               case 3:
                   return new Vector3(-5.3f, 31.2f, -15.0f);
            }
            return new Vector3(-5.3f, 0.0f, -16.3f);
        }

        public static GameObject InstantiateObject(string prefabType, List<GameObject> prefabList, Camera mainCamera, Transform interactableTransform)
    {
        // Validation checks
        if (string.IsNullOrEmpty(prefabType))
        {
            Debug.LogError("PrefabType is null or empty!");
            return null;
        }

        if (mainCamera == null)
        {
            Debug.LogError("MainCamera is null!");
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Cannot find main camera!");
                return null;
            }
        }

        if (interactableTransform == null)
        {
            Debug.LogError("InteractableTransform is null!");
            return null;
        }

        // Get prefab
        GameObject prefab = GetPrefabFromString(prefabType, prefabList);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found for type: {prefabType}");
            return null;
        }

        // Calculate spawn position
        Vector3 spawnPosition;
        try
        {
            GameObject floor = GameObject.Find("Floor") ?? GameObject.Find("floor");
            if (floor != null)
            {
                Vector3 upwardOffset = Vector3.up * 1f;
                Vector3 forwardOffset = mainCamera.transform.forward * 1f;
                spawnPosition = floor.transform.position + upwardOffset + forwardOffset;
            }
            else
            {
                spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 1f;
                Debug.LogWarning("Floor not found, using camera position as reference");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calculating spawn position: {e.Message}");
            return null;
        }

        // Instantiate object
        GameObject go = null;
        try
        {
            go = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (go == null)
            {
                Debug.LogError("Failed to instantiate object!");
                return null;
            }

            // Set parent
            go.transform.parent = interactableTransform;

            // Setup Rigidbody
            Rigidbody rigidbody = go.GetComponent<Rigidbody>() ?? go.GetComponentInChildren<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = go.AddComponent<Rigidbody>();
                Debug.Log($"Added Rigidbody to {go.name}");
            }

            rigidbody.useGravity = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            // Rename object
            int sameTypeCount = 0;
            foreach (Transform child in interactableTransform)
            {
                if (child.name.StartsWith(prefabType)) sameTypeCount++;
            }
            go.name = prefabType + sameTypeCount;

            // Set tag and ECAObject component
            go.tag = "Interactable";
            ECAObject ecaObject = go.GetComponent<ECAObject>();
            if (ecaObject != null)
            {
                ecaObject.isUsingGravity = ECABoolean.YES;
            }
            else
            {
                Debug.LogWarning($"ECAObject component not found on {go.name}");
                ecaObject = go.AddComponent<ECAObject>();
                ecaObject.isUsingGravity = ECABoolean.YES;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during object instantiation: {e.Message}");
            if (go != null)
            {
                Object.Destroy(go);
            }
            return null;
        }

        return go;
    }
        
        public static void ApplyOriginalMaterialToDuplicatedObjects(Transform interactablesTransform)
        {
            foreach (Transform go in interactablesTransform)
            {
                ECAObject ecaObjectComponent = go.GetComponent<ECAObject>();
                if (ecaObjectComponent && ecaObjectComponent.isDuplicated == ECABoolean.YES)
                {
                    ecaObjectComponent.RestoreOriginalMaterials();
                }
            }
        }

        public static void DestroySpawnedObjects(Transform interactablesTransform)
        {
            foreach (Transform go in interactablesTransform)
            {
                ECAObject ecaObjectComponent = go.GetComponent<ECAObject>();
                if (ecaObjectComponent)
                {
                    if (ecaObjectComponent.isACopy == ECABoolean.YES)
                    {
                        GameObject.Destroy(go.gameObject);
                    }
                }
            }
        }
    
        public static GameObject InstantiateSpawnObject(string prefabType, List<GameObject> prefabList, Camera mainCamera, 
        Transform interactableTransform, Vector3 position)
    {
        // Validation checks
        if (string.IsNullOrEmpty(prefabType))
        {
            Debug.LogError("PrefabType is null or empty!");
            return null;
        }

        if (mainCamera == null)
        {
            Debug.LogError("MainCamera is null!");
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Cannot find main camera!");
                return null;
            }
        }

        if (interactableTransform == null)
        {
            Debug.LogError("InteractableTransform is null!");
            return null;
        }

        // Get prefab
        GameObject prefab = GetPrefabFromString(prefabType, prefabList);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found for type: {prefabType}");
            return null;
        }

        // Calculate spawn position
        /*Vector3 spawnPosition;
        try
        {
            GameObject floor = GameObject.Find("Floor") ?? GameObject.Find("floor");
            if (floor != null)
            {
                Vector3 upwardOffset = Vector3.up * 1f;
                Vector3 forwardOffset = mainCamera.transform.forward * 2f;
                spawnPosition = position + upwardOffset + forwardOffset;
            }
            else
            {
                spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f;
                Debug.LogWarning("Floor not found, using camera position as reference");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error calculating spawn position: {e.Message}");
            return null;
        }*/

        // Instantiate object
        GameObject go = null;
        try
        {
            go = Object.Instantiate(prefab, position, Quaternion.identity);
            if (go == null)
            {
                Debug.LogError("Failed to instantiate object!");
                return null;
            }

            // Set parent
            go.transform.parent = interactableTransform;

            // Setup Rigidbody
            Rigidbody rigidbody = go.GetComponent<Rigidbody>() ?? go.GetComponentInChildren<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = go.AddComponent<Rigidbody>();
                Debug.Log($"Added Rigidbody to {go.name}");
            }

            rigidbody.useGravity = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            // Rename object
            int sameTypeCount = 0;
            foreach (Transform child in interactableTransform)
            {
                if (child.name.StartsWith(prefabType)) sameTypeCount++;
            }
            go.name = prefabType + sameTypeCount;

            // Set tag and ECAObject component
            go.tag = "Interactable";
            ECAObject ecaObject = go.GetComponent<ECAObject>();
            if (ecaObject != null)
            {
                ecaObject.isUsingGravity = ECABoolean.YES;
            }
            else
            {
                Debug.LogWarning($"ECAObject component not found on {go.name}");
                ecaObject = go.AddComponent<ECAObject>();
                ecaObject.isUsingGravity = ECABoolean.YES;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during object instantiation: {e.Message}");
            if (go != null)
            {
                Object.Destroy(go);
            }
            return null;
        }

        return go;
    }
        
    
        /*public static ECAEvent GetEventFromCube(GameObject cube, GameObject interactablesParent)
        {
            ECAEvent e = new ECAEvent(cube);
            
            GameObject frontFace = cube.transform.Find("FrontFaceRule").gameObject;
            TextMeshProUGUI subjectFront = frontFace.transform.Find("Subject").transform.Find("Image").GetComponent<TextMeshProUGUI>();
            e.Subject = subjectFront.text;
                
            TextMeshProUGUI verbFront = frontFace.transform.Find("Verb").transform.Find("Image").GetComponent<TextMeshProUGUI>();
            string[] verbAndEvent = verbFront.text.Split(' ');
            if (verbAndEvent.Length == 1)
            {
                verbAndEvent = verbFront.text.Split('\n');
            }
            e.Verb = verbAndEvent[0];
            if (verbAndEvent.Length > 1)
            {
                e.EventStr = verbAndEvent[0] + " " + verbAndEvent[1];
            }
            
                
            TextMeshProUGUI objectFront = frontFace.transform.Find("Object").transform.Find("Image").GetComponent<TextMeshProUGUI>();
            e.ObjectStr = objectFront.text;
            
            e.Texture = (Texture2D)cube.GetComponent<Renderer>().material.mainTexture;
            
            e.Modality = GetModalityFromVerb(e.Verb);
            
            e.ObjectRef = interactablesParent.transform.Find(e.ObjectStr).gameObject;

            return e;
        }*/

        public static ECAEvent GetEventFromCube(GameObject cube, List<ECAEvent> recordedEvents)
        {
            CubeController cubeController = cube.GetComponent<CubeController>();
            if (cubeController == null)
            {
                return null;
            }
            int cubeID = cubeController.cubeID;
            ECAEvent e = recordedEvents.Find(ev => ev.CubeID == cubeID);
            if (e == null)
            {
                Debug.LogError($"Event not found for cube ID {cubeID}");
            }

            return e;
        }

        public static MeanwhileEvent GetMeanwhileEventFromCube(GameObject cube, List<MeanwhileEvent> recordedMeanwhiles)
        {
            MeanwhileCubeController cubeController = cube.GetComponent<MeanwhileCubeController>();
            if (cubeController == null)
            {
                return null;
            }
            int cubeID = cubeController.cubeID;
            MeanwhileEvent @event = recordedMeanwhiles.Find(m => m.CubeID == cubeID);
            if (@event == null)
            {
                Debug.LogError($"MeanwhileEvent not found for cube ID {cubeID}");
            }

            return @event;
        }

        public static InteractionCreationController.Modalities GetModalityFromVerb(string verb)
        {
            switch (verb)
            {
                case "says":
                    return InteractionCreationController.Modalities.Speech;
                case "is near to":
                    return InteractionCreationController.Modalities.Proximity;
                case "selects":
                case "deselects":
                case "clicks":
                    return InteractionCreationController.Modalities.Touch;
                case "points":
                case "stops pointing": 
                    return InteractionCreationController.Modalities.Laser;
                case "looks":
                case "stops looking":
                    return InteractionCreationController.Modalities.Headgaze;
            }

            return InteractionCreationController.Modalities.None;
        }

        /*
         * Instantiate a cube with a rule description
         * @params: cubeLevel: 1, 2, 3 is the number of joint cubes
         */
        public static GameObject InstantiateRuleCube(GameObject cubePrefab, int cubeLevel, Vector3 position, Transform parent, Texture[] texture )
        {
            GameObject cube = Object.Instantiate(cubePrefab, position, Quaternion.Euler(0f,0f,0f), parent);
            cube.transform.rotation = Quaternion.identity;
            cube.transform.localScale = new Vector3(25, 25, 25);
            cube.transform.localPosition = position;
            cube.transform.localRotation = new Quaternion(-90.0f, cube.transform.rotation.y, cube.transform.rotation.z, cube.transform.rotation.w);
            if (cubeLevel < 2)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.mainTexture = texture[0];
                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.material = material;
                material.mainTextureScale = new Vector2(0.5f, 0.5f);
                material.mainTextureOffset = new Vector2(0.25f, 0.25f);
            }
            else
            {
                // Check if the texture array contains valid textures
                if (texture == null || texture.Length < 2 || texture[0] == null || texture[1] == null)
                {
                    Debug.LogError("Invalid texture array or missing textures!");
                    return cube;
                }
                GameObject cubeLeft = cube.transform.Find("CubeLeft").gameObject;
                GameObject cubeRight = cube.transform.Find("CubeRight").gameObject;

                Material materialLeft = new Material(Shader.Find("Standard"));
                materialLeft.mainTexture = texture[0];
                Renderer rendererLeft = cubeLeft.GetComponent<Renderer>();
                rendererLeft.material = materialLeft;
                materialLeft.mainTextureScale = new Vector2(0.5f, 0.5f);
                materialLeft.mainTextureOffset = new Vector2(0.25f, 0.25f);

                Material materialRight = new Material(Shader.Find("Standard"));
                materialRight.mainTexture = texture[1]; // Fix the assignment to materialRight
                Renderer rendererRight = cubeRight.GetComponent<Renderer>();
                rendererRight.material = materialRight;
                materialRight.mainTextureScale = new Vector2(0.5f, 0.5f);
                materialRight.mainTextureOffset = new Vector2(0.25f, 0.25f);

            }
            //if this is a cube level2, we don't need the cubeId. This will most likely change in the future
            CubeController cubeController = cube.GetComponent<CubeController>();
            if (cubeController !=null )
            {
                cubeController.cubeID = cube.GetInstanceID();
            }

            return cube;
        }

        public static Vector3 CalculatePositionInPlate(float previousZ, int eventIndex)
        {
            float zPosition;
            if (eventIndex == 6)
                previousZ = 1.41f;
            zPosition=previousZ - 0.13f;
            
            float xPosition = eventIndex < 6 ? -0.37f : -0.25f; //One or more rows
            Vector3 position = new Vector3(xPosition, -0.19f, zPosition);
            return position;
        }

        public static void GenerateTextFromCubePosition(TextMeshProUGUI textLabel, string cubeDescription, string logicalOperator)
        {
            string previousString = textLabel.text;
            //Remove the new line
            string formattedText = cubeDescription.Replace("\n", " "); 
            if(previousString == "..." || previousString=="") //if it's the first cube
                textLabel.text = formattedText;
            else
            {
                textLabel.text = previousString + " "+ logicalOperator + " " + formattedText;
            }
            
        }
        
        public static void RemoveTextFromCubePosition(TextMeshProUGUI textLabel, string cubeDescription, string locution)
        {
            string input = textLabel.text;
            
            int locutionIndex = input.IndexOf(locution, StringComparison.OrdinalIgnoreCase);
            int phraseIndex = input.IndexOf(cubeDescription, StringComparison.OrdinalIgnoreCase);

            if (phraseIndex != -1)
            {
                if (locutionIndex != -1 && locutionIndex < phraseIndex)
                {
                    // Se la locuzione viene trovata prima della frase da eliminare,
                    // rimuoviamo entrambe dalla stringa di input
                    input = input.Remove(locutionIndex, locution.Length).TrimStart(' ');
                    input = input.Remove(phraseIndex - locution.Length, cubeDescription.Length).TrimStart(' ');
                    textLabel.text = input;
                }
                else
                {
                    // Altrimenti, rimuoviamo solo la frase da eliminare
                    input = input.Remove(phraseIndex, cubeDescription.Length).TrimStart(' ');
                    textLabel.text = input;
                }
            }
            
            // Rimuove eventuale operatore logico rimasto all'inizio della stringa
            if (input.StartsWith(locution, StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(locution.Length).TrimStart(' ');
            }
            // Rimuove eventuale operatore logico rimasto alla fine della stringa
            if (input.EndsWith(locution, StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(0,input.Length-locution.Length).TrimEnd(' ');
            }
            textLabel.text = input;
        }

        public static void FillTextLabelsInCube(ECAEvent e, GameObject cube)
        {
            // Define the face names and text labels
            string[] faceNames = { "FrontFaceRule", "TopFaceRule" };
            string[] labelTexts = { e.Subject, e.Verb + " " + e.EventStr, e.ObjectStr };

            switch (e.Modality)
            {
                case InteractionCreationController.Modalities.Microgesture:
                    labelTexts[1] = e.Verb + " " + e.EventStr;
                    labelTexts[2] = "microgesture";
                    break;
                case InteractionCreationController.Modalities.Speech:
                    labelTexts[1] = "says"; //3rd person for reading
                    labelTexts[2] = "\""  + e.EventStr + "\""; //keyword
                    break; 
                case InteractionCreationController.Modalities.Proximity:
                    labelTexts[0] = e.Subject;
                    labelTexts[1] = "is near to"; //3rd person for reading
                    labelTexts[2] = e.ObjectStr;
                    break;
                case InteractionCreationController.Modalities.Controller:
                    labelTexts[1] = "presses";
                    labelTexts[2] = "trigger";
                    break;
                case InteractionCreationController.Modalities.None: // action cube
                    labelTexts[1] = e.Verb;
                    if(e.Verb.Equals("is duplicated"))
                    {
                        labelTexts[2] = e.ObjectStr + " times";
                    }
                    break;
                default:
                    labelTexts[1] = e.EventStr; 
                    break;
            }
            
            
            if (e.EventCategory == CategoryController.CategoryObjectSelected.Category)
            {
                if (!string.IsNullOrEmpty(e.ObjectStr))
                {
                    string objectNameWithoutNumber = Regex.Replace(e.ObjectStr, @"\d+$", ""); // Get the object name without the number
                    labelTexts[2] = "any " + objectNameWithoutNumber; //if it's a category, we use "any" instead of the object
                }
                else
                {
                    string subjectNameWithoutNumber = Regex.Replace(e.Subject, @"\d+$", ""); // Get the subject name without the number
                    labelTexts[2] = subjectNameWithoutNumber; //if it's a category, we use "any" instead of the object
                }
                
                //if the modality is proximity, we do the same for the subject
                if (e.Modality == InteractionCreationController.Modalities.Proximity)
                {
                    string subjectNameWithoutNumber = Regex.Replace(e.Subject, @"\d+$", ""); // Get the subject name without the number
                    labelTexts[0] = "any " + subjectNameWithoutNumber; //if it's a category, we use "any" instead of the subject
                }
            }

            // Loop through each face and fill the text labels
            foreach (string faceName in faceNames)
            {
                TextMeshProUGUI[] faceLabels = GetTextLabelsInCube(cube, faceName);

                // Fill the text labels with the appropriate text
                for (int i = 0; i < faceLabels.Length; i++)
                {
                    //To read the text better we replace the spaces with new lines, only if labelTexts[i] has more than 10 ch
                    if (labelTexts[i].Length > 10)
                    {
                        string formattedText = labelTexts[i].Replace(" ", "\n"); 
                        faceLabels[i].text =formattedText;
                    }
                    else 
                    {
                        faceLabels[i].text = labelTexts[i];
                    }
                }
            }
        }

        public static TextMeshProUGUI[] GetTextLabelsInCube(GameObject cube, string face)
        {
            Transform faceTransform = cube.transform.Find(face);
            if (faceTransform == null)
            {
                Debug.LogWarning("Face not found in the cube.");
                return new TextMeshProUGUI[0];
            }

            Transform subjectTransform = faceTransform.Find("Subject/Image");
            Transform verbTransform = faceTransform.Find("Verb/Image");
            Transform objTransform = faceTransform.Find("Object/Image");
            Transform secondVerbTransform = faceTransform.Find("SecondVerb/Image");
            Transform meanwhileTransform = faceTransform.Find("Meanwhile/Image");

            TextMeshProUGUI subject = subjectTransform?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI verb = verbTransform?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI obj = objTransform?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI secondVerbText = secondVerbTransform?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI meanwhileText = meanwhileTransform?.GetComponent<TextMeshProUGUI>();

            if (obj != null && secondVerbText != null && meanwhileText != null)
            {
                return new[] { subject, verb,  meanwhileText, secondVerbText , obj};
            }

            if (obj != null && secondVerbText != null)
            {
                return new[] { subject, verb, obj, secondVerbText };
            }

            if (obj != null)
            {
                return new[] { subject, verb, obj };
            }

            if (secondVerbText != null && meanwhileText != null)
            {
                return new[] { subject, verb, meanwhileText, secondVerbText  };
            }

            return secondVerbText != null ? new[] { subject, verb, secondVerbText } : new[] { subject, verb };
        }

        public static TextMeshProUGUI[] GetOrderedTextLabels(GameObject cube, string face)
        {
            TextMeshProUGUI[] faceLabels = GetTextLabelsInCube(cube, face);

            return faceLabels.OrderBy(label =>
            {
                switch (label.text.ToLower())
                {
                    case "subject": return 0;
                    case "verb": return 1;
                    case "and": return 2;
                    case "secondverb": return 3;
                    case "object": return 4;
                    default: return 5;
                }
            }).ToArray();
        }
        
        public static void FillTextLabelsInMergedCubes(GameObject newCube, ECAEvent [] events)
        {
            // Define the face names and text labels
            string[] faceNames = { "FrontFaceRule", "TopFaceRule" };
            
            if (events[0].EventStr == null)
            {
                events[0].EventStr = events[0].Verb;
            }
            if (events[1].EventStr == null)
            {
                events[1].EventStr = events[1].Verb;
            }

            if (events[0].Modality == InteractionCreationController.Modalities.Speech)
            {
                events[0].EventStr = events[0].Verb;
            }
            
            if (events[1].Modality == InteractionCreationController.Modalities.Speech)
            {
                events[1].EventStr = events[1].Verb;
            }
            
            events[0].EventStr = ConvertToIngForm(events[0].EventStr);
            events[1].EventStr = ConvertToIngForm(events[1].EventStr);
            
            string[] labelTexts = { events[0].Subject, events[0].EventStr + " " + events[0].ObjectStr, "and", events[1].EventStr + " ", events[1].ObjectStr };

            if(events[0].EventCategory == CategoryController.CategoryObjectSelected.Category)
            {
                string objectNameWithoutNumber = Regex.Replace(events[0].ObjectStr, @"\d+$", ""); // Get the object name without the number
                labelTexts[1] = events[0].EventStr + " " + "any " + objectNameWithoutNumber; //if it's a category, we use "any" instead of the object
            } 
            
            if(events[1].EventCategory == CategoryController.CategoryObjectSelected.Category)
            {
                string objectNameWithoutNumber = Regex.Replace(events[1].ObjectStr, @"\d+$", ""); // Get the object name without the number
                labelTexts[4] = "any " + objectNameWithoutNumber; //if it's a category, we use "any" instead of the object
            }
            
            // Loop through each face and fill the text labels
            foreach (string faceName in faceNames)
            {
                TextMeshProUGUI[] faceLabels = GetOrderedTextLabels(newCube, faceName);
                
                // Fill the text labels with the appropriate text
                for (int i = 0; i < faceLabels.Length; i++)
                {
                    faceLabels[i].text = labelTexts[i];
                }
                
            }
        } 
        
        public static string ConvertToIngForm(string verb) {
            if (string.IsNullOrEmpty(verb)) return verb;

            if (verb.StartsWith("stops") || verb.StartsWith("is")) return verb;

            Dictionary<string, string> irregularVerbs = new Dictionary<string, string>
            {
                { "say", "saying" },
                { "be", "being" }
            };

            // Handle third-person singular (remove 's' or 'es')
            if (verb.EndsWith("s") && !verb.EndsWith("ss"))
            {
                verb = verb.Substring(0, verb.Length - 1);
            }
            else if (verb.EndsWith("es") && (verb.EndsWith("ses") || verb.EndsWith("xes") || verb.EndsWith("zes") || verb.EndsWith("ches") || verb.EndsWith("shes")))
            {
                verb = verb.Substring(0, verb.Length - 2);
            }

            // Re-check for irregular verbs after modification
            if (irregularVerbs.ContainsKey(verb.ToLower()))
                return "is " + irregularVerbs[verb.ToLower()];

            if (Regex.IsMatch(verb, "[aeiou][^aeiou]e$"))
                return "is " + verb.Substring(0, verb.Length - 1) + "ing";

            if (Regex.IsMatch(verb, "[^aeiou][aeiou][^aeiou]$"))
                return "is " + verb + verb[verb.Length - 1] + "ing";

            return "is " + verb + "ing";
        }



        public static string GetRuleDescriptionFromCubePrefab(GameObject cube)
        {
            string ruleDescription = "";
            //Front face
            TextMeshProUGUI [] frontFaceR = GetTextLabelsInCube(cube, "FrontFaceRule");
            foreach (var t in frontFaceR)
            {
                ruleDescription += t.text + " ";
            }

            return ruleDescription;
        }

        public static void ClearTextDescription(TextMeshProUGUI whenText, TextMeshProUGUI thenText)
        {
            whenText.text = "...";
            thenText.text = "...";
        }

        //Delete all the unnecessary containers
        public static void ResetCubeContainers()
        {
            Transform whenContainer = GameObject.FindGameObjectsWithTag("RuleUtils").FirstOrDefault(x => x.name == "When").transform;
            Transform whenFrontplate = whenContainer.Find("Frontplate");
            GameObject whenSequentialRow = whenFrontplate.Find("SequentialRow").gameObject;
            GameObject whenEquivalenceRow = whenFrontplate.Find("EquivalenceRow").gameObject;
            //Delete all the gameobject that are called "Cube Container (Clone)"
            foreach (Transform child in whenSequentialRow.transform)
            {
                if (child.name.StartsWith("CubeContainer(Clone)")) Object.Destroy(child.gameObject);
            }

            foreach (Transform child in whenEquivalenceRow.transform)
            {
                if (child.name.StartsWith("CubeContainer(Clone)")) Object.Destroy(child.gameObject);
            }
            
            Transform thenContainer = GameObject.FindGameObjectsWithTag("RuleUtils").FirstOrDefault(x => x.name == "Then").transform;
            Transform thenFrontplate = thenContainer.Find("Frontplate");
            GameObject thenSequentialRow = thenFrontplate.Find("SequentialRow").gameObject;
            GameObject thenEquivalenceRow = thenFrontplate.Find("EquivalenceRow").gameObject;
            //Delete all the gameobject that are called "Cube Container (Clone)"
            foreach (Transform child in thenSequentialRow.transform)
            {
                if (child.name.StartsWith("CubeContainer(Clone)")) Object.Destroy(child.gameObject);
            }
            
            foreach (Transform child in thenEquivalenceRow.transform)
            {
                if (child.name.StartsWith("CubeContainer(Clone)")) Object.Destroy(child.gameObject);
            }
        }

        //Returns the category of each gameobject. E.g. cube --> shape, cheese --> food
        public static string GetECALastScriptFromECAObject(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents(typeof(MonoBehaviour));
            string lastECAComponentName = null;

            foreach (var component in components)
            {
                string componentFullName = component.GetType().ToString();
                int lastDotIndex = componentFullName.LastIndexOf('.');
                string componentName = componentFullName.Substring(lastDotIndex + 1);

                if (componentName.StartsWith("ECA"))
                {
                    lastECAComponentName = componentName;
                }
            }

            if (lastECAComponentName != null)
            {
                return lastECAComponentName.Substring(3); // remove "ECA"
            }

            return "Shape"; // fallback
        }


        public static string GetIconForECACategory(string category)
        {
            switch (category)
            {
                case "Character":
                    return "Icon 117";
                case "shape":
                case "Shape":
                    return "Icon 133";
                case "Food":
                case "Prop":    
                    return "Icon 54";
                case "Environment":
                case "Furniture":
                    return "Assets/Resources/door.png";
                case "Music":
                    return "Icon 22";
                case "Light":
                    return "Icon 90";
                case "Text":
                    return "Assets/Resources/Icons/font.png";
                case "Counter":
                    return "Assets/Resources/Icons/counter.png";
                case "Animal":
                    return "Assets/Resources/Icons/paws.png";
            }
            Debug.LogError("Icon null for category "+ category);
            return null;
        }

        public static ECAEvent ConvertActionToECAEvent(Action action)
        {
            ECAEvent e = new ECAEvent(action.GetSubject(), action.GetActionMethod());
            e.Action = action;
            e.IsActionEvent = true;
            switch (action.GetActionType())
            {
                case Action.ActionType.INVALID:
                    Debug.Log("Action INVALID");
                    break;
                case Action.ActionType.CHANGES:
                case Action.ActionType.CUSTOMCHANGE:
                    e.Verb = action.GetActionMethod() + " " + action.GetObject() + " " +  action.GetModifier();
                    e.ObjectStr = action.GetModifierValue().ToString();
                    break;
                case Action.ActionType.VALUE:
                    e.Verb = action.GetActionMethod();
                    e.ObjectStr = action.GetObject().ToString();
                    break;
                case Action.ActionType.VERB:
                    break;
                case Action.ActionType.OBJECT:
                    if (action.GetModifierValue() != null)
                    {
                        e.ObjectStr = action.GetModifierValue().ToString();
                    }
                    else
                    {
                        GameObject actionObject = action.GetObject() as GameObject;
                        e.ObjectStr = actionObject.name;
                    }
                    break;
            }

            return e;
        }

        public static Action GetOppositeAction(Action action, String ecaEventVerb)
        {
            // TODO inserire tutti gli altri
            switch (ecaEventVerb)
            {
                case "hides":
                    return new Action(action.GetSubject(), "shows");
                case "opens":
                    return new Action(action.GetSubject(), "closes");  
                case "closes":
                    return new Action(action.GetSubject(), "opens");
                case "gravityON":
                    return new Action(action.GetSubject(), "gravityOFF");
                case "gravityOFF":
                    return new Action(action.GetSubject(), "gravityON");
                case "turns":
                    string modifier = action.GetObject().ToString();
                    ECABoolean oppositeModifier = modifier.Equals("on") ? ECABoolean.OFF : ECABoolean.ON ;
                    return new Action(action.GetSubject(), "turns", oppositeModifier);
                case "changes":
                    return new Action(action.GetSubject(), "changes", action.GetModifier(), "to", action.GetModifierValue());
                case "changes color to":
                    //TODO: implement the previous color
                    ECAColor ECAColor = new ECAColor("white");
                    return new Action(action.GetSubject(), "changes", "color", "to", ECAColor);
                case "is duplicated":
                    if (GeneralUIController.Instance.UIstate != GeneralUIController.UIState.EditMode)
                    {
                        return new Action(action.GetSubject(), "delete duplicates");
                    }
                    break;
                case "follows":
                    return new Action(action.GetSubject(), "unfollows");
                case "changes text": 
                    return new Action(action.GetSubject(), "resets text");
                case "increases by one":
                case "doubles":
                case "decreases by one":
                    return new Action(action.GetSubject(), "resets counter");
                case "is thrown":
                case "explodes": 
                case "moves near":
                    return new Action(action.GetSubject(), "resets");
                case "turns off":
                    return new Action(action.GetSubject(), "turns on");
                case "turns on":
                    return new Action(action.GetSubject(), "turns off");
            }

            return null;
        }
        
        public static Action GetActionFromString(string s, GameObject SelectedObject)
        {
            if (ECAColor.IsEcaColor(s))
            {
                ECAColor color = new ECAColor(s);
                return (new Action(SelectedObject, "changes", "color", "to", color));
            }
            // get the verb by making the string s lowercase
            string verb = s.ToLower();
            return new Action(SelectedObject, verb);
        }
        
        public static void ChangeButtonAppearance(bool active, GameObject button)
        {
            GameObject frontPlate = button.transform.Find("Frontplate").gameObject;
            GameObject animatedContent = frontPlate.transform.Find("AnimatedContent").gameObject;
            var textMeshPro = animatedContent.transform.Find("Text");
            var icon = animatedContent.transform.Find("Icon");
            if(textMeshPro == null) textMeshPro = icon.transform.Find("Text");    
            var textMeshProUGUI = textMeshPro.GetComponent<TextMeshProUGUI>();
            var color = textMeshProUGUI.color;
            color.a = active ? 1 : 0.2f;
            textMeshProUGUI.color = color;
            TextMeshProUGUI fontIcon = icon.transform.Find("UIButtonFontIcon").GetComponent<TextMeshProUGUI>();
            var iconColor = fontIcon.color;
            iconColor.a = active ? 1 : 0.2f;
            fontIcon.color = iconColor;
        }

        public static void TogglePressableButton(bool active, GameObject button)
        {
            PressableButton pressableButton = button.GetComponent<PressableButton>();
            if(pressableButton){
                pressableButton.enabled = active;
            }
        }

        public static GameObject[] FindObjectsWithECAScript(GameObject parent, string scriptName)
        {
            List<GameObject> matchingObjects = new List<GameObject>();
            string fullScriptName = "ECAPrototyping.RuleEngine." + scriptName;

            foreach (Transform child in parent.transform)
            {
                Type scriptType = Type.GetType(fullScriptName);
                if (scriptType != null && child.gameObject.GetComponent(scriptType) != null)
                {
                    matchingObjects.Add(child.gameObject);
                }
            }

            return matchingObjects.ToArray();
        }

        public static void ExecuteActionOnCategory(RuleEngine _ruleEngine, Action action, GameObject parent)
        {
            GameObject mainGameObject = action.GetSubject();
            string ecaLastScript = "ECA"+ GetECALastScriptFromECAObject(mainGameObject);
            GameObject[] categoryGameObjects = FindObjectsWithECAScript(parent, ecaLastScript);
            
            foreach (GameObject categoryGameObject in categoryGameObjects)
            {   
                Action newAction = action;
                newAction.SetSubject(categoryGameObject);
                _ruleEngine.ExecuteAction(newAction);
            }
        }

        public static void ExecuteActionOnAllObjects(RuleEngine _ruleEngine, Action action, GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                Action newAction = action;
                GameObject go = child.gameObject;
                newAction.SetSubject(go);
                _ruleEngine.ExecuteAction(newAction);
            }
        }
        
        public static bool AreControllersConnected()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);
            return devices.Count > 0;
        }
    }
}