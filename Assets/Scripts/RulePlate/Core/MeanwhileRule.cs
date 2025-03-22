using System.Collections.Generic;
using UnityEngine;

namespace UI.RuleEditor
{
    public class MeanwhileRule
    {
        public ECAEvent[] events;
        public float timeWindow = 5.0f;
        public float timer;
        public int CubeID;
        private HashSet<ECAEvent> triggeredEvents = new HashSet<ECAEvent>();

        public bool TimerRunning { get; private set; }        
        public bool IsComplete => triggeredEvents.Count == events.Length;

        public MeanwhileRule(ECAEvent[] events)
        {
            this.events = events;
            timer = 0f;
            TimerRunning = false;
        }

        public void Reset()
        {
            triggeredEvents.Clear();
            timer = 0f;
            TimerRunning = false;
        }

        public void RegisterTrigger(ECAEvent ecaEvent)
        {
            triggeredEvents.Add(ecaEvent);
        }

        public bool HasTriggered(ECAEvent ecaEvent)
        {
            return triggeredEvents.Contains(ecaEvent);
        }
        
        public void StartTimer()
        {
            timer = timeWindow;
            TimerRunning = true;
        }
        
        public void UpdateTimer(float deltaTime)
        {
            if (!TimerRunning) return;

            timer -= deltaTime;

            if (timer <= 0f)
            {
                Debug.Log("Meanwhile rule timer expired. Resetting rule.");
                Reset();
            }
        }
    }

}