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
using UnityEngine.XR.Interaction.Toolkit;
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
    public int cubeID;
    [SerializeField] private float snapDistance = 0.6f;
    private Rigidbody _rigidbody;
    private Collider[] _colliders;
    private ObjectManipulator _manipulator;
    
    private void Start()
    {
        _combineRulesController = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<CombineRulesController>();
        interactables = _combineRulesController.interactables;

        _rigidbody = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _manipulator = GetComponent<ObjectManipulator>();

        // Ensure all colliders are on the same layer as the cube root.
        var cubeLayer = gameObject.layer;
        foreach (var collider in _colliders)
        {
            if (collider != null)
            {
                collider.gameObject.layer = cubeLayer;
            }
        }

        if (_manipulator != null)
        {
            _manipulator.selectEntered.AddListener(OnSelectEntered);
            _manipulator.selectExited.AddListener(OnSelectExited);
        }

        LogPhysicsState("Start");
    }

    private void OnDestroy()
    {
        if (_manipulator != null)
        {
            _manipulator.selectEntered.RemoveListener(OnSelectEntered);
            _manipulator.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs _)
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbody.detectCollisions = true;
        }
        LogPhysicsState("SelectEntered");
    }

    private void OnSelectExited(SelectExitEventArgs _)
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.detectCollisions = true;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();
        LogPhysicsState("SelectExited");
        TrySnapToNearestContainer();
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
        CubeController otherCubeController = otherCube.GetComponent<CubeController>();
        if (otherCubeController != true) yield break;
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
        
        ECAEvent cubeLeftEcaEvent = Utils.GetEventFromCube(gameObject, GeneralUIController.Instance.recordedEvents);
        ECAEvent cubeRightEcaEvent = Utils.GetEventFromCube(otherCube, GeneralUIController.Instance.recordedEvents);
                
        // Get the texture of a gameobject
        Texture textureLeftCube = gameObject.GetComponent<Renderer>().material.mainTexture;
        Texture textureRightCube = otherCube.GetComponent<Renderer>().material.mainTexture;
        
        Texture copyTextureLeftCube = CopyTexture(textureLeftCube);
        Texture copyTextureRightCube = CopyTexture(textureRightCube);
        
        // Mark the other cube as attached to prevent double merge
        otherCube.GetComponent<CubeController>().isAttached = true;
        
        GameObject mergedCube = Utils.InstantiateRuleCube(mergedCubePrefab, 2, mergedPosition, cubePlate.transform, new []{copyTextureLeftCube, copyTextureRightCube});

        mergedCube.transform.rotation = new Quaternion(0, -190f, 0, 0);
        mergedCube.transform.localScale = new Vector3(25, 25, 25);
        mergedCube.transform.localPosition = mergedPosition;
        mergedCube.transform.localPosition = new Vector3(mergedCube.transform.position.x, mergedCube.transform.position.y, -36.8f);

        Utils.FillTextLabelsInMergedCubes(mergedCube, new []{cubeLeftEcaEvent, cubeRightEcaEvent});

        _combineRulesController.DeactivateRuleDebugText();
        
        MeanwhileEvent @event = new MeanwhileEvent(new [] {cubeLeftEcaEvent, cubeRightEcaEvent});
        @event.CubeID = mergedCube.GetInstanceID();
        GeneralUIController.Instance.activeMeanwhileEvents.Add(@event);

        MeanwhileCubeController meanwhileCubeController = mergedCube.AddComponent<MeanwhileCubeController>();
        meanwhileCubeController.cubeID = mergedCube.GetInstanceID();
        
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

    private void LogPhysicsState(string label)
    {
        var rbState = _rigidbody != null
            ? $"rb(kinematic={_rigidbody.isKinematic}, gravity={_rigidbody.useGravity}, detectCollisions={_rigidbody.detectCollisions})"
            : "rb(null)";
        var colliderState = _colliders != null && _colliders.Length > 0
            ? string.Join(", ", _colliders.Select(c => $"{c.name}(enabled={c.enabled}, trigger={c.isTrigger}, layer={LayerMask.LayerToName(c.gameObject.layer)})"))
            : "colliders(none)";
        Debug.Log($"[CubeController] {label} {name} layer={LayerMask.LayerToName(gameObject.layer)} {rbState} {colliderState}");
    }

    private void TrySnapToNearestContainer()
    {
        var containers = FindObjectsOfType<CubeContainer>();
        if (containers.Length == 0)
        {
            Debug.LogWarning("[CubeController] No CubeContainer found for snap.");
            return;
        }

        CubeContainer best = null;
        float bestSqrDistance = float.MaxValue;
        var cubePosition = transform.position;

        foreach (var container in containers)
        {
            var collider = container.GetComponent<Collider>();
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            var sqrDistance = collider.bounds.SqrDistance(cubePosition);
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                best = container;
            }
        }

        if (best == null)
        {
            Debug.LogWarning("[CubeController] No valid CubeContainer collider found for snap.");
            return;
        }

        if (bestSqrDistance > snapDistance * snapDistance)
        {
            Debug.Log($"[CubeController] Nearest container too far. sqrDist={bestSqrDistance:F3} threshold={snapDistance * snapDistance:F3}");
            return;
        }

        Debug.Log($"[CubeController] Snap attempt to {best.name} (sqrDist={bestSqrDistance:F3})");
        best.TryAttachCube(gameObject);
    }
    
}
