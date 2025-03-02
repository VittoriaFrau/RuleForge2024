using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UI;
using UI.RuleEditor;
using UnityEngine;
using UnityEngine.Diagnostics;
using Object = UnityEngine.Object;
using Utils = UI.Utils;

public class CubeController : MonoBehaviour
{
    private bool isAttached = false;
    private bool timerStarted = false;
    
    public float minimumJoinTime = 2.0f; // Minimum time (in seconds) for the cubes to stay attached
    private float joinStartTime;
    public GameObject mergedCubePrefab;
    private CombineRulesController _combineRulesController;
    private GameObject interactables;
    
    private void Start()
    {
        _combineRulesController = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<CombineRulesController>();
        interactables = _combineRulesController.interactables;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAttached && collision.gameObject.CompareTag("RuleCubes"))
        {
            // Register the time when the collision started
            joinStartTime = Time.time;
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        int numberOfCollidingObjects = collision.contactCount;
        
        if (!isAttached && CheckTags(collision.gameObject) && numberOfCollidingObjects == 1)
        {
            if (!timerStarted)
            {
                // Start the countdown timer when collision starts
                StartCoroutine(StartCountdown(collision.gameObject));
            }
            
            if (Time.time - joinStartTime >= minimumJoinTime)
            {
                // Merge the cubes if the minimum join time has passed
                MergeCubes(collision.gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!isAttached && CheckTags(other.gameObject))
        {
            // Reset the countdown if the cubes are no longer colliding
            timerStarted = false;
            _combineRulesController.DeactivateRuleDebugText();
        }
    }

    //Returns true if the tags are compatibles
    bool CheckTags(GameObject g1)
    {
        if (gameObject.CompareTag("RuleCubes") && g1.gameObject.CompareTag("RuleCubes"))
        {
            return true;
        }

        return false;
    }
    
    private IEnumerator StartCountdown(GameObject otherCube)
    {
        timerStarted = true;
        otherCube.GetComponent<CubeController>().timerStarted = true;
        float countdownTime = minimumJoinTime;
        while (countdownTime > 0)
        {
            // Update the UI Text to show the countdown
            _combineRulesController.ActivateDebugTextWithMessage("Merging in " + Mathf.CeilToInt(countdownTime) + " seconds");
            yield return null;
            countdownTime -= Time.deltaTime;
        }

        // Reset the countdown
        timerStarted = false;
        _combineRulesController.DeactivateRuleDebugText();
    }

    private void MergeCubes(GameObject otherCube)
    {
        //if one of the cubes has the tag action cube, return
        if (gameObject.CompareTag("ActionRuleCube") || otherCube.CompareTag("ActionRuleCube"))
        {
            return;
        }
        // Prevent further collisions while the cubes are being merged
        isAttached = true;

        // Disable object manipulators on both cubes
        GetComponent<ObjectManipulator>().enabled = false;
        otherCube.GetComponent<ObjectManipulator>().enabled = false;

        // Position and merge the cubes
        Vector3 mergedPosition = (transform.position + otherCube.transform.position) / 2f;

        // Finding the cubeplate by tag and then filtering the results by name
        GameObject cubePlate = GameObject.FindGameObjectsWithTag("RuleUtils").FirstOrDefault(x => x.name == "CubePlate");
        
        ECAEvent cubeLeftEcaEvent = Utils.GetEventFromCube(gameObject, interactables);
        ECAEvent cubeRightEcaEvent = Utils.GetEventFromCube(otherCube, interactables);
                
        // Get the texture of a gameobject
        Texture textureLeftCube = gameObject.GetComponent<Renderer>().material.mainTexture;
        Texture textureRightCube = otherCube.GetComponent<Renderer>().material.mainTexture;
        
        Texture copyTextureLeftCube = CopyTexture(textureLeftCube);
        Texture copyTextureRightCube = CopyTexture(textureRightCube);
        
        // Mark the other cube as attached to prevent double merge
        otherCube.GetComponent<CubeController>().isAttached = true;
        
        GameObject mergedCube = Utils.InstantiateRuleCube(mergedCubePrefab, 2, mergedPosition, cubePlate.transform, new []{copyTextureLeftCube, copyTextureRightCube});

        mergedCube.transform.rotation = Quaternion.identity;
        mergedCube.transform.localScale = new Vector3(25, 25, 25);
        mergedCube.transform.localPosition = mergedPosition;
        mergedCube.transform.localPosition = new Vector3(mergedCube.transform.position.x, mergedCube.transform.position.y, -36.8f);

        Utils.FillTextLabelsInMergedCubes(mergedCube, new []{cubeLeftEcaEvent, cubeRightEcaEvent});

        _combineRulesController.DeactivateRuleDebugText();
        
        // Destroy both original cubes 
        Destroy(gameObject);
        Destroy(otherCube);
    }
    
    // Helper method to copy a texture
    private Texture CopyTexture(Texture originalTexture)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(originalTexture, tempRT);
        Texture2D copyTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = tempRT;
        copyTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        copyTexture.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempRT);
        return copyTexture;
    }
    
}
