using System.Collections;
using UI;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    
        /// <summary>
        /// A <b>Door</b> is a type of ECAEnvironment, it represents a door.
        /// </summary>
        [DisallowMultipleComponent]
        [ECARules4All("door")]
        [RequireComponent(typeof(ECAEnvironment))]

        public class ECADoor : MonoBehaviour
        {
            /// <summary>
            /// <b>isOpen</b> represents the state of the door.
            /// By default, the door is closed.
            /// </summary>
            public bool isOpen = false;
            private bool rotating = false;
            private Transform _doorTransform;
            private GameObject _doorChild;
            private const float rotationDuration = 1.0f;
            
            //Event to notify when the notation is ended
            public event System.Action DoorOpened;
        
            /// <summary>
            /// <b> Color </b> is the color of the object 
            /// </summary>
            [StateVariable("isOpen", ECARules4AllType.Boolean)] 
            
            private void Awake()
            {
                isOpen = false;
                /*_doorChild = gameObject.transform.GetChild(0).gameObject;
                _doorTransform = _doorChild.transform;*/
                
            }
            
            /// <summary>
            /// <b>opens</b> opens the door.
            ///  </summary>
            [Action(typeof(ECADoor), "opens")]
            public void OpenDoor()
            {
                /*isOpen = true;
                //Deactivate component HingeJoint
                HingeJoint hingeJoint = gameObject.GetComponent<HingeJoint>();
                Destroy(hingeJoint);
                //Rotate the door with time.deltatime
                StartRotation(gameObject, new Vector3(0, 50, 0));*/

                if (!isOpen && !rotating)
                {
                    isOpen = true;
                    HingeJoint hingeJoint = gameObject.GetComponent<HingeJoint>();
                    Destroy(hingeJoint);

                    // Usa StartCoroutine per attendere che la rotazione sia completata
                    StartCoroutine(Rotate(new Vector3(0, 50, 0), gameObject, () =>
                    {
                        GameObject eventHandler = GameObject.FindWithTag("EventHandler");
                        GeneralUIController generalUIController = eventHandler.GetComponent<GeneralUIController>();
                        if (generalUIController.isRecording)
                        {
                            Action action = new Action(this.gameObject, "opens");
                            generalUIController.InteractionCreationController.RecordActionPressedButton(action, this.gameObject);
                        }

                    }));
                }
            }
            
            /// <summary>
            /// <b>closes</b> closes the door.
            /// </summary>
            
            [Action(typeof(ECADoor), "closes")]
            public void CloseDoor()
            {
                isOpen = false;
                //Deactivate component HingeJoint
                HingeJoint hingeJoint = gameObject.GetComponent<HingeJoint>();
                if(hingeJoint)
                    Destroy(hingeJoint);
                //Rotate the door with time.deltatime
                StartCoroutine(Rotate(new Vector3(0, -50, 0), gameObject, () =>
                {
                    GameObject eventHandler = GameObject.FindWithTag("EventHandler");
                    GeneralUIController generalUIController = eventHandler.GetComponent<GeneralUIController>();
                    if (generalUIController.isRecording)
                    {
                        Action action = new Action(this.gameObject, "closes");
                        generalUIController.InteractionCreationController.RecordActionPressedButton(action, this.gameObject);
                    }
                    
                }));

            }
            
            private IEnumerator Rotate( Vector3 angles, GameObject objectToRotate, System.Action onComplete )
            {
                rotating = true ;
                Quaternion startRotation = objectToRotate.transform.rotation ;
                Quaternion endRotation = Quaternion.Euler( angles ) * startRotation ;
                for( float t = 0 ; t < rotationDuration ; t+= Time.deltaTime )
                {
                    objectToRotate.transform.rotation = Quaternion.Lerp( startRotation, endRotation, t / rotationDuration ) ;
                    yield return null;
                }
                objectToRotate.transform.rotation = endRotation  ;
                rotating = false;
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            }

            /*public void StartRotation(GameObject objectToRotate, Vector3 rotation)
            {
                if (!rotating)
                {
                    StartCoroutine( Rotate( rotation, objectToRotate ) ) ;
                    
                }
                    
            }*/
            
            private System.Collections.IEnumerator RotateCoroutine()
            {
                yield return new WaitForSeconds(2f);
                
            }

            
            private System.Collections.IEnumerator ResetInstantiation()
            {
                yield return new WaitForSeconds(2f);
                
            }

        }
    
}