using System;
using UnityEngine;

public class Button_Interaction : MonoBehaviour
{
    
    Renderer objectRenderer;
    Renderer childRenderer;
    public WirePuzzleLogic logic;

    void Start()
    {
        //Debug.Log("ButtonPressed script started.");
        objectRenderer = GetComponent<Renderer>();
        childRenderer = GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
        {
            Debug.Log("Renderer component found on parent.");
        }
        if (childRenderer != null)
        {
            Debug.Log("Renderer component found on child.");
        }  
    }
    public void OnButtonPress()
    {
        //Debug.Log("Sending button " + gameObject.tag + " to WirePuzzleLogic");
        if (logic != null)
        {
            logic.OnButtonPressed(gameObject.tag);
        }
    }
}
