using Oculus.Interaction;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class LogInteractor : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textElement;
    [SerializeField]
    private SnapInteractable snapInteractable;

    [SerializeField]
    private LogManager logManager;

    public void GetSelectingInteractorId()
    {
        if (snapInteractable.SelectingInteractorViews.Any())
        {
            UpdatePanelText();
        }
        else
        {
            Debug.Log("No interactor is currently selecting this interactable.");
        }
    }

    public void UpdatePanelText()
    {

        foreach (IInteractorView interactorView in snapInteractable.SelectingInteractorViews)
        {
            var mb = interactorView as MonoBehaviour;
            string objName = mb != null ? mb.gameObject.name : "Unknown";
            if (logManager != null)
            {
                logManager.AddSnappedName(objName);
            }
        }
    }
}