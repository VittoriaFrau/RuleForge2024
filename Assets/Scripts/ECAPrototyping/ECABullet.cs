using UnityEngine;

namespace ECAPrototyping.RuleEngine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECAObject))]
    [ECARules4All("bullet")]
    public class ECABullet : MonoBehaviour
    {
    }
    
}