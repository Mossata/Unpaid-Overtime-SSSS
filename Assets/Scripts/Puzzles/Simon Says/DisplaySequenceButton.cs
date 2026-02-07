using UnityEngine;
public class DisplaySequenceButton : MonoBehaviour
{
    public SimonLogic simon;

    void Start()
    {
        Debug.Log("ButtonPressed script started.");
    }

    public void ShowSequence()
    {
        Debug.Log("Next Sequence button pressed.");
        // Send tag to InitialiseSimon
        if (simon != null)
        {
            simon.DisplayAgain();
        }
    }
}