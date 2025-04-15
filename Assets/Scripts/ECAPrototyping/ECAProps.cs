using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("props")]
    public class ECAProps : MonoBehaviour
    {
        [Action(typeof(ECAProps), "turn on conveyor")]
        public void TurnOnConveyor()
        {
            GetComponent<Animator>().SetTrigger("Play");
        }
        
        [Action(typeof(ECAProps), "turn off conveyor")]
        public void TurnOffConveyor()
        {
            GetComponent<Animator>().ResetTrigger("Play");
            GetComponent<Animator>().Play("Idle");
        }
        
        [Action(typeof(ECAProps), "change speed")]
        public void ChangeSpeed()
        {
            //todo
        }

        
    }
}