using System;
using UnityEngine;

public class ButtonPressed : MonoBehaviour
{
    public SimonLogic simon;

    void Start()
    {
        Debug.Log("ButtonPressed script started.");
    }

    public void ChangeColor()
    {
        Debug.Log("Simon Says button pressed.");
        // Send tag to InitialiseSimon
        if (simon != null)
        {
            simon.OnButtonSelected(gameObject.tag);
        }
    }
}