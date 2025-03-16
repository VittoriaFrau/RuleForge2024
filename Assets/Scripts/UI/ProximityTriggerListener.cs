namespace UI
{
    using UnityEngine;
    using System;

    public class ProximityTriggerListener : MonoBehaviour
    {
        // Public event that other classes can subscribe to
        public event Action<Collider> OnProximityEnter;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"OnTriggerEnter detected on {gameObject.name} with {other.name}");

            OnProximityEnter?.Invoke(other);
        }
    }
}