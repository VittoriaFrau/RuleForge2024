using System.Collections.Generic;
using UI;
using UI.RuleEditor;

namespace RulePlate.Core
{
    public class ECARule
    {
        List<ECAEvent> events;
        public List<ECAEvent> Events
        {
            get => events;
            set => events = value;
        }
        
        List<ECAEvent> actions;
        public List<ECAEvent> Actions
        {
            get => actions;
            set => actions = value;
        }
        
        List<MeanwhileEvent> meanwhileEvents;
        public List<MeanwhileEvent> MeanwhileEvents
        {
            get => meanwhileEvents;
            set => meanwhileEvents = value;
        }
        
        
        
        public ECARule(List<ECAEvent> events, List<ECAEvent> actions)
        {
            this.events = events;
            this.actions = actions;
        }
        
        public ECARule(List<MeanwhileEvent> meanwhileEvents, List<ECAEvent> actions )
        {
            this.actions = actions;
            this.meanwhileEvents = meanwhileEvents;
        }
        
        //toString method to display the rule in the console
        public override string ToString()
        {
            string ruleString = "Rule: \n";
            if(events != null && events.Count > 0)
            {
                ruleString += "Events: ";
                foreach (var e in events)
                {
                    ruleString += e.ToString() + "\n";
                }
            }
            if (meanwhileEvents != null && meanwhileEvents.Count > 0)
            {
                ruleString += "Meanwhile Events: ";
                foreach (var me in meanwhileEvents)
                {
                    ruleString += me.ToString() + "\n";
                }
            }
            ruleString += "Actions: ";
            foreach (var a in actions)
            {
                ruleString += a.ToString() + "\n";
            }
            
            return ruleString;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is ECARule otherRule)
            {
                return (events == otherRule.events && actions == otherRule.actions) ||
                       (events == otherRule.events && meanwhileEvents == otherRule.meanwhileEvents 
                                                   && actions == otherRule.actions);
            }
            return false;
        }
        
    }
}