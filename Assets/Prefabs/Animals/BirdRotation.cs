using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(new Vector3(-90.0f, 0.0f, 180.0f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
