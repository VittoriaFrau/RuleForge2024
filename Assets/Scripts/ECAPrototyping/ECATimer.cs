using TMPro;
using UI;
using UnityEngine;
namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [RequireComponent(typeof(TextMeshPro))]
    [ECARules4All("timer")]
    public class ECATimer : MonoBehaviour
    {

        [Action(typeof(ECATimer), "resets timer")]
        public void ResetTimer()
        {
            GeneralUIController.Instance.InteractionCreationController.ResetTimer();
        }
    }
}