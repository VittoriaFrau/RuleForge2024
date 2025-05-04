using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("prop")]
    public class ECAProp : MonoBehaviour
    {
        [Action(typeof(ECAProp), "turns on")]
        public void TurnOnConveyor()
        {
            GetComponent<Animator>().SetTrigger("Play");
        }
        
        [Action(typeof(ECAProp), "turns off")]
        public void TurnOffConveyor()
        {
            GetComponent<Animator>().ResetTrigger("Play");
            GetComponent<Animator>().Play("Idle");
        }
        
        [Action(typeof(ECAProp), "increases speed")]
        public void Increases()
        {
            //todo
        }

        
    }
}