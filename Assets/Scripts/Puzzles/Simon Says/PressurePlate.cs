using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour
{

    Renderer objectRenderer;

    void Start()
    {
        GameObject Donut = GameObject.FindWithTag("Donut");
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            Debug.Log("Renderer component found.");
        }
        else
        {
            Debug.LogError("Renderer component not found!");
        }

    }

    void Update()
    {

    }

    // Reduce player hp if collides with enemy
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Donut"))
        {
            ChangeColor();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Donut"))
        {
            ChangeColor();
        }
    }
    
    private void ChangeColor()
    {
        Debug.Log("Changing color");
        if (objectRenderer.material.color == Color.red)
        {
            objectRenderer.material.color = Color.green;
        }
        else
        {
            objectRenderer.material.color = Color.red;
        }
    }
}