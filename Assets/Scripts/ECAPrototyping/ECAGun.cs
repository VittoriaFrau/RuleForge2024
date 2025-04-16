using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("gun")]
    public class ECAGun : MonoBehaviour
    {
    }
    
}