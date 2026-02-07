using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WireCut : MonoBehaviour
{
    public GameObject wireGameObject;
    public WireCutController wireController;
    public GameObject wire;
    private bool isWireCut = false;
    private Vector3 originalWirePosition;

    void Start()
    {
        if (wire != null)
        {
            originalWirePosition = wire.transform.position;
        }
    }

    public void CutWire()
    {
        if (!isWireCut)
        {
            Debug.Log("Wire cut!");
            if (wireGameObject != null)
            {
                wireGameObject.SetActive(false);
            }
            if (wire != null)
            {
                wire.transform.position += new Vector3(0, 0, 0.02f);
            }
            isWireCut = true;
            SendWireCutSignal();
        }
        else
        {
            Debug.Log("Wire reset to original position!");

            if (wireGameObject != null)
            {
                wireGameObject.SetActive(true);
            }
            if (wire != null)
            {
                wire.transform.position = originalWirePosition;
            }
            isWireCut = false;
        }
    }

    public void SendWireCutSignal()
    {
        Debug.Log(wireGameObject.tag + " cut.");

        if (wireController != null)
        {
            wireController.OnButtonSelected(wireGameObject.tag);
        }
    }

    public void ResetWire()
    {
        if (isWireCut)
        {
            Debug.Log("Resetting wire to original state");

            if (wireGameObject != null)
            {
                wireGameObject.SetActive(true);
            }
            if (wire != null)
            {
                wire.transform.position -= new Vector3(0, 0, 0.02f);
            }
            isWireCut = false;
        }
    }
}