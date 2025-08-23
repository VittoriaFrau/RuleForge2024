using Controllers;
using ECAPrototyping.RuleEngine;
using JetBrains.Annotations;
using UnityEngine;

namespace UI
{
    public class ECAEvent
    {
        private InteractionCreationController.Modalities modality;
        public CategoryController.CategoryObjectSelected ObjectCategory { get; set; }
        public CategoryController.CategoryObjectSelected SubjectCategory { get; set; }
        private static int _counter = 0;
        
        public string Subject { get; set; }
        public string Verb { get; set; }
        public string ObjectStr{ get; set; }
        public GameObject ObjectRef{ get; set; } // reference to the gameobject of the object part of the rule
        public Texture2D Texture{ get; set; }
        public string EventStr{ get; set; } //string of the event in natural language
        public int CubeID{ get; set; } //ID of the cube, same of the CubeController
        public Action Action { get; set; } // Only for action cubes
        
        public string VariableName { get; set; } // Name of the variable, if any
        
        public Vector3 CubeInitialPosition { get; set; }
        public bool IsActionEvent { get; set; }
        public bool IsEquivalenceEvent { get; set; }


        
        public ECAEvent(GameObject @object, InteractionCreationController.Modalities modality, string _event, 
            [CanBeNull] Texture2D screenshot, bool isActionEvent)
        {
            this.ObjectRef = @object;
            this.modality = modality;
            this.EventStr = _event;
            ObjectCategory = GeneralUIController.Instance.CategoryController.lastCategorySelected;
            SubjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
            if(screenshot != null) Texture = screenshot;
            else Texture = null;
            IsActionEvent = isActionEvent;
            SetModalityRule();
            _counter++;
        }


        //Proximity
        public ECAEvent(GameObject @object, InteractionCreationController.Modalities modality, string _event,
            GameObject targetObject, [CanBeNull] Texture2D screenshot, bool isActionEvent)
        {
            this.ObjectRef = targetObject;
            this.modality = modality;
            this.EventStr = _event;
            if (screenshot != null) Texture = screenshot;
            else Texture = null;
            IsActionEvent = isActionEvent;
            Subject = @object.name;
            Verb = "is near to";
            ObjectStr = ObjectRef.name;
            _counter++;
            ObjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
            SubjectCategory = GeneralUIController.Instance.CategoryController.lastCategorySelected;
        }
        
        // Timer
        public ECAEvent(GameObject @object, InteractionCreationController.Modalities modality, string _event, int nSeconds,
            [CanBeNull] Texture2D screenshot, bool isActionEvent)
        {
            this.ObjectRef = @object;
            this.modality = modality;
            this.EventStr = _event;
            ObjectStr = "" + nSeconds;
            ObjectCategory = CategoryController.CategoryObjectSelected.SingleObject; //By default
            SubjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
            if(screenshot != null) Texture = screenshot;
            else Texture = null;
            IsActionEvent = isActionEvent;
            SetModalityRule();
            _counter++;
        }
        
        public ECAEvent(GameObject @object)
        {
            ObjectRef = @object;
            _counter++;
            ObjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
            SubjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
        }
        
        public ECAEvent(GameObject @object, string verb)
        {
            ObjectRef = @object;
            Verb = verb;
            Subject = @object.name;
            _counter++;
            ObjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
            SubjectCategory = CategoryController.CategoryObjectSelected.SingleObject;
        }

        public void ChangeSubjectCategory(CategoryController.CategoryObjectSelected category)
        {
            SubjectCategory = category;
        }

        public void ChangeObjectCategory(CategoryController.CategoryObjectSelected category)
        {
            ObjectCategory = category;
        }

        public InteractionCreationController.Modalities Modality
        {
            get => modality;
            set => modality = value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not ECAEvent e)
            {
                return false;
            }

            if (e.modality != InteractionCreationController.Modalities.None)
            {
                // If it's category, we need to check the category and the subject
                if (e.ObjectCategory == CategoryController.CategoryObjectSelected.Category)
                {
                    if (Subject == e.Subject)
                    {
                        if (modality == e.modality)
                        {
                            var latestECAScriptE = Utils.GetECALastScriptFromECAObject(e.ObjectRef);
                            var latestECAScriptThis = Utils.GetECALastScriptFromECAObject(ObjectRef);
                            if (latestECAScriptE != null && latestECAScriptThis != null)
                            {
                                return latestECAScriptE.Equals(latestECAScriptThis);
                            }
                        }
                    }
                }
            }

            if (!ReferenceEquals(ObjectRef, e.ObjectRef))
            {
                return false;
            }

            if (ToString().Equals(e.ToString()))
            {
                return true;
            }

            if (Verb != null && e.Verb != null)
            {
                if (!ReferenceEquals(ObjectStr, e.ObjectStr))
                {
                    return false;
                }

                return Verb == e.Verb;
            }

            if (modality == InteractionCreationController.Modalities.Timer)
            {
                return ObjectStr == e.ObjectStr;
            }

            // ModalityEvent:
            return ObjectRef == e.ObjectRef && modality == e.modality && EventStr == e.EventStr;
        }


        public override string ToString()
        {
            
            if (modality != InteractionCreationController.Modalities.None)
            {
                return ToStringModalityEvent();
            }
            return ToStringActionEvent();
        }

        private string ToStringModalityEvent()
        {
            
            // Handle specific modalities 
            switch (modality)
            {
                case InteractionCreationController.Modalities.Microgesture:
                    return $"The user performs {EventStr} microgesture";

                case InteractionCreationController.Modalities.Speech:
                    return $"The user says {EventStr}";

                case InteractionCreationController.Modalities.Controller when ObjectRef == null:
                    return "The user presses the trigger";
                
                case InteractionCreationController.Modalities.Proximity:
                    return Subject + Verb + ObjectStr;
                
                case InteractionCreationController.Modalities.Timer:
                    return Subject + " " + Verb + " " + ObjectStr + " seconds";
            }

            if (EventStr != null && Verb != null && EventStr != null)
            {
                return "The user " + EventStr + " " + Verb + " the " + ObjectRef.name + " object";
            }
            if (Verb != null) return "The user " + Verb + " the " + ObjectRef.name + " object";
            if(EventStr == null && Verb == null) return "The user " + modality + " the " + ObjectRef.name + " object";
            
            return ObjectRef.name + " " + Verb + " " + ObjectStr;

        }

        private string ToStringActionEvent()
        {
            if(ObjectStr == null && ObjectRef == null)
            {
                return Subject + " " + Verb;
            }
            
            if(ObjectStr == null) return ObjectRef.name + " " + Verb;
            
            if(ObjectRef == null)
            {
                return Subject + " " + Verb + " " + ObjectStr;
            }
            return ObjectRef.name + " " + Verb + " " + ObjectStr;
        }

        private void SetModalityRule()
        {
            Subject = "user";
            switch (modality)
            {
                case InteractionCreationController.Modalities.Microgesture:
                    Verb = "performs";
                    ObjectStr = EventStr;
                    break;
                case InteractionCreationController.Modalities.Speech:
                    Verb = "says";
                    ObjectStr = EventStr;
                    break;
                case InteractionCreationController.Modalities.Timer:
                    Subject = ObjectRef.name;
                    Verb = "hits";
                    break;
                default:   
                    Verb = modality.ToString();
                    if(ObjectRef) ObjectStr = ObjectRef.name;
                    break;
            }
        }
        

    }
}