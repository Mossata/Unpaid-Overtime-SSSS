using System;
using UnityEngine;

public class Fan_Rotation : MonoBehaviour
{
    public float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        // Rotate the fan around its Y-axis
        transform.Rotate (Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
