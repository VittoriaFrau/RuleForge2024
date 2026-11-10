using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Diagnostics;
using Utils = UI.Utils;

//TODO cancel?
public class MergedCubeController : MonoBehaviour
{
    private static bool timerStarted = false;

    // Detach countdown (separate from CubeController.minimumJoinTime, the merge hold time): how
    // long the user must pull the two halves apart before they detach. Currently 2s, unchanged by
    // the Task 3 merge-time update (paper only specifies the 3s merge hold, not a detach time).
    // Note: this component is not attached to any prefab/scene today and DetachCubes has no
    // caller (verified by searching the whole repo) - the detach flow appears unused/unfinished.
    public float detachHoldSeconds = 2.0f;
    private float joinStartTime;
    
    private TextMeshProUGUI countdownText;
    
    private void Start()
    {
        GameObject debugTextObject = GameObject.FindGameObjectWithTag("debugText");
        // Verifica se l'oggetto è stato trovato e non è null
        if (debugTextObject != null)
        {
            // Ottieni il componente TextMeshProUGUI solo se l'oggetto è attivo
            if (debugTextObject.activeSelf)
            {
                countdownText = debugTextObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                // L'oggetto è inattivo, puoi gestire questo caso qui se necessario
                Debug.LogWarning("L'oggetto con tag 'debugText' è inattivo.");
            }
        }
    }

    public void DetachCubes()
    {
        // Remove the joint to detach the cubes
       
            if (!timerStarted && countdownText != null)
            {
                // Start the countdown timer when collision starts
                StartCoroutine(StartCountdownDetaching());
            }
            
            if (Time.time - joinStartTime >= detachHoldSeconds)
            {
                // Merge the cubes if the minimum join time has passed
                Debug.Log("Detach cubes");
            }
        
    }

    private IEnumerator StartCountdownDetaching()
    {
        timerStarted = true;
        float countdownTime = detachHoldSeconds;
        while (countdownTime > 0)
        {
            // Update the UI Text to show the countdown
            countdownText.text = "Detaching in " + Mathf.CeilToInt(countdownTime) + " seconds";
            yield return null;
            countdownTime -= Time.deltaTime;
        }

        // Reset the countdown
        timerStarted = false;
    }
}
