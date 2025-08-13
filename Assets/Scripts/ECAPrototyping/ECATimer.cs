using System;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("timer")]
    public class ECATimer : MonoBehaviour
    {

        private float timerDuration; 
        private float currentTime;
        private TextMeshPro timerText;
        private Action<ECAEvent> trackerCallback;
        private ECAEvent trackerEvent;
        private Coroutine activeTimerCoroutine = null;


        private void Start()
        {
            timerText = gameObject.GetComponentInChildren<TextMeshPro>();
            timerDuration = 0f;
            currentTime = 0f;
            timerText.fontSize = 2.5f;
            timerText.color = Color.white;
            timerText.text = "0:00";
        }
        
        public void SetTimerDuration(float duration)
        {
            timerDuration = duration;
            currentTime = 0f;
            if (timerText != null)
            {
                timerText.text = UI.Utils.CalculateTimerText(duration);
            }
        }

        [Action(typeof(ECATimer), "restarts")]
        public void Restart()
        {
            currentTime = 0;
            timerText.text = "0:00";
            if (GeneralUIController.Instance.UIstate == GeneralUIController.UIState.Play)
            {
                StartTimer(trackerCallback, trackerEvent);
            }
        }
        
        public void StartTimer(Action<ECAEvent> notifyTracker = null, ECAEvent ecaEvent = null)
        {
            if (activeTimerCoroutine != null)
            {
                StopCoroutine(activeTimerCoroutine);
                activeTimerCoroutine = null;
            }

            trackerCallback = notifyTracker;
            trackerEvent = ecaEvent;
            activeTimerCoroutine = StartCoroutine(TimerCoroutine());
        }
        
        public void StopTimer()
        {
            if (activeTimerCoroutine != null)
            {
                StopCoroutine(activeTimerCoroutine);
                activeTimerCoroutine = null;
            }

            if (timerText != null)
            {
                timerText.text = UI.Utils.CalculateTimerText(timerDuration);
            }
        }

        
        public void ResetTimerToZero()
        {
            currentTime = 0f;
            if (timerText != null)
            {
                timerText.text = UI.Utils.CalculateTimerText(0f);
            }
        }

        private IEnumerator TimerCoroutine()
        {
            float timeElapsed = 0f;

            while (timeElapsed <= timerDuration)
            {
                if (timerText != null)
                {
                    // Mostra il valore attuale (senza arrotondamenti strani)
                    timerText.text = Mathf.FloorToInt(timeElapsed).ToString();
                }

                yield return new WaitForSeconds(1f);
                timeElapsed += 1f;
            }
            trackerCallback?.Invoke(trackerEvent);
        }
        
    }
}