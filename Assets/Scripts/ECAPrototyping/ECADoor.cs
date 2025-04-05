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
        private Transform doorTransform; // Riferimento al transform di 01_low
        private const float rotationDuration = 1.0f;

        //Event to notify when the door action is ended
        public event System.Action DoorOpened;

        /// <summary>
        /// <b> Color </b> is the color of the object 
        /// </summary>
        [StateVariable("isOpen", ECARules4AllType.Boolean)] 

        private void Awake()
        {
            isOpen = false;
            // Trova il GameObject 01_low (la porta effettiva)
            Transform doorPart = transform.Find("01_low");
            if (doorPart != null)
            {
                doorTransform = doorPart;
                
                // Aggiungi qui il codice per il posizionamento
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        bounds.Encapsulate(renderers[i].bounds);
                    }

                    transform.position = new Vector3(
                        transform.position.x,
                        0,
                        transform.position.z
                    );
                }
            }
            else
            {
                Debug.LogError("Component 01_low not found in door prefab!");
            }
        }

        /// <summary>
        /// <b>opens</b> opens the door.
        /// </summary>
        [Action(typeof(ECADoor), "opens")]
        public void OpenDoor()
        {
            if (!isOpen && !rotating && doorTransform != null)
            {
                isOpen = true;
                
                // Rimuovi l'HingeJoint se presente
                HingeJoint hingeJoint = doorTransform.GetComponent<HingeJoint>();
                if (hingeJoint != null)
                {
                    Destroy(hingeJoint);
                }

                // Avvia la rotazione solo per il componente porta
                StartCoroutine(Rotate(new Vector3(0, 90, 0), doorTransform.gameObject, () =>
                {
                    // Callback dopo la rotazione
                    GameObject eventHandler = GameObject.FindWithTag("EventHandler");
                    if (eventHandler != null)
                    {
                        GeneralUIController generalUIController = eventHandler.GetComponent<GeneralUIController>();
                        if (GeneralUIController.Instance != null && GeneralUIController.Instance.isRecording)
                        {
                            Action action = new Action(this.gameObject, "opens");
                            generalUIController.InteractionCreationController.SaveRecordedAction(action);
                        }
                    }
                    
                    // Notifica che la porta è stata aperta
                    DoorOpened?.Invoke();
                }));
            }
        }

        /// <summary>
        /// <b>closes</b> closes the door.
        /// </summary>
        [Action(typeof(ECADoor), "closes")]
        public void CloseDoor()
        {
            if (isOpen && !rotating && doorTransform != null)
            {
                isOpen = false;

                // Rimuovi l'HingeJoint se presente
                HingeJoint hingeJoint = doorTransform.GetComponent<HingeJoint>();
                if (hingeJoint != null)
                {
                    Destroy(hingeJoint);
                }

                // Avvia la rotazione inversa solo per il componente porta
                StartCoroutine(Rotate(new Vector3(0, -90, 0), doorTransform.gameObject, () =>
                {
                    // Callback dopo la rotazione
                    GameObject eventHandler = GameObject.FindWithTag("EventHandler");
                    if (eventHandler != null)
                    {
                        GeneralUIController generalUIController = eventHandler.GetComponent<GeneralUIController>();
                        if (GeneralUIController.Instance != null && GeneralUIController.Instance.isRecording)
                        {
                            Action action = new Action(this.gameObject, "closes");
                            generalUIController.InteractionCreationController.SaveRecordedAction(action);
                        }
                    }
                }));
            }
        }

        private IEnumerator Rotate(Vector3 angles, GameObject objectToRotate, System.Action onComplete)
        {
            rotating = true;
            Quaternion startRotation = objectToRotate.transform.localRotation;
            Quaternion endRotation = Quaternion.Euler(angles) * startRotation;

            for (float t = 0; t < rotationDuration; t += Time.deltaTime)
            {
                objectToRotate.transform.localRotation = Quaternion.Lerp(startRotation, endRotation, t / rotationDuration);
                yield return null;
            }

            objectToRotate.transform.localRotation = endRotation;
            rotating = false;

            // Chiamata del callback di completamento
            onComplete?.Invoke();
        }

        /// <summary>
        /// Verifica se la porta è attualmente in fase di rotazione
        /// </summary>
        public bool IsRotating()
        {
            return rotating;
        }

        /// <summary>
        /// Verifica se la porta è aperta
        /// </summary>
        public bool IsOpen()
        {
            return isOpen;
        }
    }
}