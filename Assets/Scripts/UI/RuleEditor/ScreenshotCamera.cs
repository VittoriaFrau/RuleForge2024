using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.RuleEditor
{
    public class ScreenshotCamera:MonoBehaviour
    {
        private Camera secondaryCamera, mainCamera;

        public GameObject interactablesContainer;
        public int resWidth = 2550; 
        public int resHeight = 2550;
        private List<GameObject> interactableGameObjects;

        private void Start()
        {
            secondaryCamera = this.GetComponent<Camera>();
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            interactableGameObjects = new List<GameObject>();
        }

        public void TakeModalityScreenshot(GameObject gameObject, InteractionCreationController.Modalities modality, ECAEvent ecaEvent)
        {
            GetInteractableGameObjects();
            
            HideOtherGameobjects(gameObject);
            
            switch (modality)
            {
                case InteractionCreationController.Modalities.Headgaze:
                    //Use the main camera
                    CaptureImageFromCamera(secondaryCamera, ecaEvent);
                    break;
                case InteractionCreationController.Modalities.Laser:
                    PositionSecondaryCameraInFrontOfObject(gameObject, secondaryCamera, mainCamera);
                    CaptureImageFromCamera(secondaryCamera, ecaEvent);
                    break;
                case InteractionCreationController.Modalities.Touch:
                    CaptureImageFromCamera(secondaryCamera, ecaEvent);
                    break;
            }

            //TEST --> disattivato per il test perchè mostro un gameobject alla volta
            //ShowGameobjects(gameObject);
        }

        public void TakeActionScreenshot(GameObject gameObject, ECAEvent ecaEvent)
        {
            GetInteractableGameObjects();
            //TEST --> disattivato per il test perchè mostro un gameobject alla volta
            //HideOtherGameobjects(gameObject);
            secondaryCamera.gameObject.SetActive(true);
            PositionSecondaryCameraInFrontOfObject(gameObject, secondaryCamera, mainCamera);
            CaptureImageFromCamera(secondaryCamera, ecaEvent);
            secondaryCamera.gameObject.SetActive(false);
            //TEST --> disattivato per il test perchè mostro un gameobject alla volta
            //ShowGameobjects(gameObject);
        }

        public static void PositionSecondaryCameraInFrontOfObject(GameObject gameObject, Camera secondaryCamera, Camera mainCamera)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            
            //Position the camera
            float cameraDistance = 2.0f; // Constant factor
            Vector3 objectSizes = collider.bounds.max - collider.bounds.min;
            float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * secondaryCamera.fieldOfView); // Visible height 1 meter in front
            float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
            distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
            secondaryCamera.transform.position = collider.bounds.center - distance * secondaryCamera.transform.forward;
            secondaryCamera.transform.rotation = mainCamera.transform.rotation;
        }
        
        public static string ScreenShotName(int width, int height) {
            return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png", 
                Application.dataPath, 
                width, height, 
                System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }


        private void HideOtherGameobjects(GameObject gameObject)
        {
            //Hide all the other objects except the one we are capturing to take the screenshot
            for (int i = 0; i < interactableGameObjects.Count; i++)
            {
                GameObject child = interactableGameObjects[i];
                if (child != gameObject && child != gameObject.transform.parent.gameObject)
                {
                    child.SetActive(false);
                }
            }
        }

        private void ShowGameobjects(GameObject gameObject)
        {
            for (int i = 0; i < interactableGameObjects.Count; i++)
            {
                GameObject child = interactableGameObjects[i];
                if(child != gameObject)
                    child.SetActive(true);
            }
        }

        /*private void CaptureImageFromCamera(Camera camera, ECAEvent ecaEvent)
        {
            //Take screenshot
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            ecaEvent.Texture = screenShot;
            //Decomment for saving the screenshot to file
            /*byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));#1#
        }*/
        
        private void CaptureImageFromCamera(Camera camera, ECAEvent ecaEvent)
        {
            camera.gameObject.SetActive(true);
            // Take screenshot
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            screenShot.Apply();
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            ecaEvent.Texture = screenShot;
            camera.gameObject.SetActive(false);
        }

        /**
         * Updates the list of the current interactable gameobjects
         */
        public void GetInteractableGameObjects()
        {
            if(interactableGameObjects==null) SetUpCamera();
            interactableGameObjects.Clear();
            for (int i = 0; i < interactablesContainer.transform.childCount; i++)
            {
                GameObject child = interactablesContainer.transform.GetChild(i).gameObject;
                interactableGameObjects.Add(child);
            }
        }

        private void SetUpCamera()
        {
            secondaryCamera = this.GetComponent<Camera>();
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            interactableGameObjects = new List<GameObject>();
        }
    }
}