using System;
using System.Collections.Generic;
using ECAPrototyping.RuleEngine;
using UI;
using UnityEngine;

namespace Controllers
{
    public class TimersController: MonoBehaviour
    {
        private List<GameObject> timerList = new();

        public void BindTimerEvent(string timerObjName, float durationInSeconds, Action<ECAEvent> notifyTracker, ECAEvent timerEvent)
        {
            GameObject timerObj = timerList.Find(timer => timer.name.Equals(timerObjName, StringComparison.OrdinalIgnoreCase));
            if (timerObj != null)
            {
                ECATimer ecaTimer = timerObj.GetComponent<ECATimer>();
                if (ecaTimer != null)
                {
                    ecaTimer.SetTimerDuration(durationInSeconds);
                    ecaTimer.StartTimer(notifyTracker, timerEvent); // Passo la callback e l'evento
                }
                else
                {
                    Debug.LogError($"ECATimer component not found on {timerObjName}");
                }
            }
            else
            {
                Debug.LogError($"Timer object with name {timerObjName} not found in timerList.");
            }
        }
        
        public void UnBindAllTimerEvents()
        {
            foreach (var timer in timerList)
            {
                if (timer.TryGetComponent<ECATimer>(out var ecaTimer))
                {
                    ecaTimer.StopTimer();
                }
            }
        }

        
        public void AddTimer(GameObject timer)
        {
            if (timer != null && !timerList.Contains(timer))
            {
                timerList.Add(timer);
            }
        }
        
        public void RemoveTimer(GameObject timer)
        {
            if (timer != null && timerList.Contains(timer))
            {
                timerList.Remove(timer);
            }
        }
        
        public void ResetAllTimersToZero()
        {
            foreach (var timer in timerList)
            {
                if (timer.TryGetComponent<ECATimer>(out var ecaTimer))
                {
                    ecaTimer.ResetTimerToZero();
                }
            }
        }

        public void StopAllTimers()
        {
            foreach (var timer in timerList)
            {
                if (timer.TryGetComponent<ECATimer>(out var ecaTimer))
                {
                    ecaTimer.StopTimer();
                }
            }
        }
    }
}