using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("props")]
    public class ECAProps : MonoBehaviour
    {
        [Action(typeof(ECAProps), "turns on")]
        public void TurnOnConveyor()
        {
            GetComponent<Animator>().SetTrigger("Play");
        }
        
        [Action(typeof(ECAProps), "turns off")]
        public void TurnOffConveyor()
        {
            GetComponent<Animator>().ResetTrigger("Play");
            GetComponent<Animator>().Play("Idle");
        }
        
        [Action(typeof(ECAProps), "increases speed")]
        public void Increases()
        {
            //todo
        }

        
    }
}