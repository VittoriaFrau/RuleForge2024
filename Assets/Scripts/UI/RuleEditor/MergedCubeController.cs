using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.SpatialManipulation;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Diagnostics;
using Utils = UI.Utils;

public class MergedCubeController : MonoBehaviour
{
    private static bool timerStarted = false;
    

    public bool TimerStarted
    {
        get => timerStarted;
        set => timerStarted = value;
    }
    
    public float minimumJoinTime = 2.0f; // Minimum time (in seconds) for the cubes to stay attached
    private float joinStartTime;
    
    private TextMeshProUGUI countdownText;
    private ObjectManipulator objectManipulator;
    
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
        
        objectManipulator = GetComponent<ObjectManipulator>();
        
        
    }

    public void DetachCubes()
    {
        // Remove the joint to detach the cubes
       
            if (!timerStarted && countdownText != null)
            {
                // Start the countdown timer when collision starts
                StartCoroutine(StartCountdownDetaching());
            }
            
            if (Time.time - joinStartTime >= minimumJoinTime)
            {
                // Merge the cubes if the minimum join time has passed
                Debug.Log("Detach cubes");
            }
        
    }

    private IEnumerator StartCountdownDetaching()
    {
        timerStarted = true;
        float countdownTime = minimumJoinTime;
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
