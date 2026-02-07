using UnityEngine;
using TMPro;
using UnityEngine.Android;
using System.Collections;
using UnityEngine.SceneManagement;


public class MonitorLogic : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI dotsText;
    public TextMeshProUGUI textElement;
    private bool isCycling = false; 

    void Start()
    {
        isCycling = true;
        StartCoroutine(DotCycleSequence());
    }

    private System.Collections.IEnumerator DotCycleSequence()
    {
        while (isCycling)
        {
            dotsText.text = ".";
            yield return new WaitForSeconds(1f);
            dotsText.text = "..";
            yield return new WaitForSeconds(1f);
            dotsText.text = "...";
            yield return new WaitForSeconds(1f);
        }
        
    }
}