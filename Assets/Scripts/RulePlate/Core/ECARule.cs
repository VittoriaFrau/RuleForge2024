using System.Collections.Generic;
using Controllers;
using UI;
using UI.RuleEditor;
using UnityEngine;

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

        public void MarkDynamicSubjects()
        {
            if (Events == null || Actions == null) return;

            foreach (var when in Events)
            {
                if (when.ObjectCategory == CategoryController.CategoryObjectSelected.Category)
                {
                    string categoryNameSubject = (when.Subject.Equals("user") ? "user" : Utils.GetECALastScriptFromECAObject(GameObject.Find(when.Subject))); 
                    string categoryNameObject = when.ObjectRef == null ? "" : Utils.GetECALastScriptFromECAObject(when.ObjectRef); 
                    
                    foreach (var then in Actions)
                    {
                        var action = then.Action;
                        var target = action.GetSubject();
                        string targetCategoryName = Utils.GetECALastScriptFromECAObject(target);

                        // Caso 1: target nullo (nessun oggetto specifico impostato)
                        if (target == null)
                        {
                            action.IsDynamicSubject = true;
                            Debug.Log($"[ECARule] Marked action '{then.Verb}' as dynamic (no specific target, category: '{targetCategoryName}')");
                        }
                        // Caso 2: il target ha un tag o tipo che matcha la categoria
                        else if (targetCategoryName == categoryNameSubject || targetCategoryName == categoryNameObject){
                            action.IsDynamicSubject = true;
                            Debug.Log($"[ECARule] Marked action '{then.Verb}' as dynamic (tag match: '{targetCategoryName}')");
                        }
                    }
                }
            }
        }

        
    }
}