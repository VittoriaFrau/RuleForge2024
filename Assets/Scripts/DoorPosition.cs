using UnityEngine;

public class DoorPosition : MonoBehaviour
{
    [SerializeField]
    private float additionalHeightOffset = -0.5f; // Valore negativo per abbassare la porta
    
    [SerializeField]
    private float rotationY = 90f; // Rotazione di 90 gradi

    private void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // Calcola l'altezza considerando l'offset aggiuntivo
            float heightOffset = (bounds.size.y * 0.5f) + additionalHeightOffset;

            // Posiziona la porta
            transform.position = new Vector3(
                transform.position.x,
                heightOffset,
                transform.position.z
            );

            // Ruota la porta di 90 gradi sull'asse Y
            transform.rotation = Quaternion.Euler(0, rotationY, 0);

            // Gestione del Rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log($"Porta posizionata a Y: {heightOffset} con rotazione: {rotationY}");
        }
    }
}
