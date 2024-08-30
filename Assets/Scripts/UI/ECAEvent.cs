using ECAPrototyping.RuleEngine;
using UnityEngine;

namespace UI
{
    public class ECAEvent
    {
        private GameObject _gameObject;
        private InteractionCreationController.Modalities modality;
        private string _event;
        //TODO: sto selezionando cubo, shape o un qualsiasi oggetto?
        private InteractionCreationController.CategoryObjectSelected typeOfObject;
        private Texture2D _texture;
        private string _subject, _verb, _object;
        private Action _action;
        private int _index;
        private string cubeID; //Per recuperare il cubo generato

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public string CubeID
        {
            get => cubeID;
            set => cubeID = value;
        }

        private static int _counter = 0;

        public Action Action
        {
            get => _action;
            set => _action = value;
        }

        public ECAEvent(GameObject gameObject, string _verb, string _object)
        {
            _gameObject = gameObject;
            this._verb = _verb;
            this._object = _object;
            this._subject = gameObject.name;
            _index = _counter;
            _counter++;
        }
        
        public ECAEvent(GameObject gameObject, InteractionCreationController.Modalities modality, string _event, 
            Texture2D screenshot)
        {
            this._gameObject = gameObject;
            this.modality = modality;
            this._event = _event;
            typeOfObject = InteractionCreationController.CategoryObjectSelected.GameObject; //By default
            _texture = screenshot;
            SetModalityRule();
            _index = _counter;
            _counter++;
        }
        
        public ECAEvent(GameObject gameObject, InteractionCreationController.Modalities modality, string _event)
        {
            this._gameObject = gameObject;
            this.modality = modality;
            this._event = _event;
            typeOfObject = InteractionCreationController.CategoryObjectSelected.GameObject; //By default
            _texture = null;
            SetModalityRule();
            _index = _counter;
            _counter++;
        }

        public ECAEvent(GameObject gameObject)
        {
            _gameObject = gameObject;
            _index = _counter;
            _counter++;
        }
        
        public ECAEvent(GameObject gameObject, string verb)
        {
            _gameObject = gameObject;
            _verb = verb;
            _subject = gameObject.name;
            _index = _counter;
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

            if (!ReferenceEquals(_gameObject, e._gameObject))
            {
                return false;
            }

            if (ToString().Equals(e.ToString()))
            {
                return true;
            }

            if (_verb != null && e._verb != null)
            {
                if (!ReferenceEquals(_object, e._object))
                {
                    return false;
                }

                return _verb == e._verb;
            }

            // ModalityEvent:
            return _gameObject == e.GameObject && modality == e.modality && _event == e._event;
        }


        public override string ToString()
        {
            if (modality == InteractionCreationController.Modalities.Microgesture)
            {
                return "The user performs " + _event + " microgesture";
            }
            if(modality == InteractionCreationController.Modalities.Speech)
            {
                return "The user says " + _event;
            }

            if (modality != InteractionCreationController.Modalities.None)
            {
                if (_event != null && _verb != null && _event != null)
                {
                    return "The user " + _event + " " + _verb + " the " + _gameObject.name + " object";
                }
                if (_verb != null) return "The user " + _verb + " the " + _gameObject.name + " object";
                if(_event == null && _verb == null) return "The user " + modality + " the " + _gameObject.name + " object";
            }
            if(_object == null) return _gameObject.name + " " + _verb;
            
            return _gameObject.name + " " + _verb + " " + _object;
        }

        private void SetModalityRule()
        {
            _subject = "user";
            switch (modality)
            {
                case InteractionCreationController.Modalities.Microgesture:
                    _verb = "performs";
                    _object = _event;
                    break;
                case InteractionCreationController.Modalities.Speech:
                    _verb = "says";
                    _object = _event;
                    break;
                default:   
                    _verb = modality.ToString();
                    _object = _gameObject.name;
                    break;
            }
        }
        
        public string Subject
        {
            get => _subject;
            set => _subject = value;
        }
        
        public string Verb
        {
            get => _verb;
            set => _verb = value;
        }
        
        public string Object
        {
            get => _object;
            set => _object = value;
        }
        
        public GameObject GameObject
        {
            get => _gameObject;
            set => _gameObject = value;
        }
        
        public Texture2D Texture
        {
            get => _texture;
            set => _texture = value;
        }
        
        public string Event
        {
            get => _event;
            set => _event = value;
        }

    }
}