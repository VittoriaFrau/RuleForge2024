using TMPro;
using UI;
using UnityEngine;
namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("timer")]
    public class ECATimer : MonoBehaviour
    {

        [Action(typeof(ECATimer), "restarts")]
        public void Restart()
        {
            GeneralUIController.Instance.InteractionCreationController.ResetTimer();
        }
    }
}