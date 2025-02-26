using ECAPrototyping.RuleEngine;
using JetBrains.Annotations;
using UnityEngine;

namespace UI
{
    public class ECAEvent
    {
        private InteractionCreationController.Modalities modality;
        //TODO: sto selezionando cubo, shape o un qualsiasi oggetto?
        private InteractionCreationController.CategoryObjectSelected typeOfObject;
        private static int _counter = 0;
        
        public string Subject { get; set; }
        public string Verb { get; set; }
        public string ObjectStr{ get; set; }
        public GameObject GameObjectRef{ get; set; }
        public Texture2D Texture{ get; set; }
        public string EventStr{ get; set; }
        public string CubeID{ get; set; }
        public int Index{ get; set; }
        public Action Action { get; set; }
        
        public ECAEvent(GameObject gameObject, InteractionCreationController.Modalities modality, string _event, 
            [CanBeNull] Texture2D screenshot)
        {
            this.GameObjectRef = gameObject;
            this.modality = modality;
            this.EventStr = _event;
            typeOfObject = InteractionCreationController.CategoryObjectSelected.GameObject; //By default
            if(screenshot != null) Texture = screenshot;
            else Texture = null;
            SetModalityRule();
            Index = _counter;
            _counter++;
        }
        
        public ECAEvent(GameObject gameObject)
        {
            GameObjectRef = gameObject;
            Index = _counter;
            _counter++;
        }
        
        public ECAEvent(GameObject gameObject, string verb)
        {
            GameObjectRef = gameObject;
            Verb = verb;
            Subject = gameObject.name;
            Index = _counter;
            _counter++;
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

            if (!ReferenceEquals(GameObjectRef, e.GameObjectRef))
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

            // ModalityEvent:
            return GameObjectRef == e.GameObjectRef && modality == e.modality && EventStr == e.EventStr;
        }


        public override string ToString()
        {
            if (modality == InteractionCreationController.Modalities.Microgesture)
            {
                return "The user performs " + EventStr + " microgesture";
            }
            if(modality == InteractionCreationController.Modalities.Speech)
            {
                return "The user says " + EventStr;
            }

            if (modality != InteractionCreationController.Modalities.None)
            {
                if (EventStr != null && Verb != null && EventStr != null)
                {
                    return "The user " + EventStr + " " + Verb + " the " + GameObjectRef.name + " object";
                }
                if (Verb != null) return "The user " + Verb + " the " + GameObjectRef.name + " object";
                if(EventStr == null && Verb == null) return "The user " + modality + " the " + GameObjectRef.name + " object";
            }
            if(ObjectStr == null) return GameObjectRef.name + " " + Verb;
            
            return GameObjectRef.name + " " + Verb + " " + ObjectStr;
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
                default:   
                    Verb = modality.ToString();
                    ObjectStr = GameObjectRef.name;
                    break;
            }
        }

    }
}