using ECAPrototyping.Utils;
using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("prop")]
    public class ECAProp : MonoBehaviour
    {

        [StateVariable("equipable", ECARules4AllType.Boolean)]
        public ECABoolean isEquipable = new(ECABoolean.BoolType.NO);

        private float speedFactor = 1f;
        private float speedStep = 0.8f;

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
        public void IncreasesSpeed()
        {
            speedFactor += speedStep;
            GetComponent<Animator>().speed = speedFactor;
        }


        [Action(typeof(ECAProp), "decreases speed")]
        public void DecreasesSpeed()
        {
            speedFactor = Mathf.Max(0f, speedFactor - speedStep); // never go below 0
            GetComponent<Animator>().speed = speedFactor;
        }

        [ContextMenu("Do Something")]
        public void DoSomething()
        {
            DecreasesSpeed();
        }

        
    }
}