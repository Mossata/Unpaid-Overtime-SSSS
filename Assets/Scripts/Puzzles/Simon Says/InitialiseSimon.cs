using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class InitialiseSimon : MonoBehaviour
{

    Renderer objectRenderer;
    Renderer childRenderer;
    Color[] colours = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };

    
    public GameObject[] buttons;
    Color[] shuffledColours;
    public string[] selectedTags = new string[4];
    Renderer simonRenderer;
    bool allButtonsFound;

    public Dictionary<Color, string> colorToButtonTag = new Dictionary<Color, string>
    {
        { Color.red, "RedButton" },
        { Color.green, "GreenButton" },
        { Color.blue, "BlueButton" },
        { Color.yellow, "YellowButton" }
    };

    void Start()
    {

        GameObject RedButton = GameObject.FindWithTag("RedButton");
        GameObject GreenButton = GameObject.FindWithTag("GreenButton");
        GameObject BlueButton = GameObject.FindWithTag("BlueButton");
        GameObject YellowButton = GameObject.FindWithTag("YellowButton");

        buttons = new GameObject[] { RedButton, GreenButton, BlueButton, YellowButton };

        allButtonsFound = true;
        foreach (GameObject button in buttons)
        {
            if (button == null)
            {
                allButtonsFound = false;
                break;
            }
        }
        if (allButtonsFound)
        {
            Debug.Log("All buttons found.");
        }
        else
        {
            Debug.LogError("One or more buttons not found!");
        }

    }
    public void OnButtonSelected(string buttonTag)
    {
        Debug.Log("Button with tag " + buttonTag + " was selected.");

        for (int i = 0; i < selectedTags.Length; i++)
        {
            if (string.IsNullOrEmpty(selectedTags[i]))
            {
                Debug.Log("Storing tag at position " + i);
                selectedTags[i] = buttonTag;
                break;
            }
        }
        if (selectedTags[3] != "")
        {
            Debug.Log("All buttons selected: " + string.Join(", ", selectedTags));
            EvaluateAnswer();
        }
    }

    public void StartSimon()
    {
        Debug.Log("Simon Game Started");
        ClearSelectedTags();
        Transform simonChild = transform.Find("Simon");
        if (simonChild != null)
        {
            simonRenderer = simonChild.GetComponent<Renderer>();
            if (simonRenderer != null)
            {
                simonRenderer.material.color = Color.black;
            }
            else
            {
                Debug.LogWarning("Renderer not found on Simon");
            }
        }
        else
        {
            Debug.LogWarning("Simon not found.");
        }
        if (allButtonsFound && buttons.Length > 0)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                Transform quadChild = buttons[i].transform.Find("Quad");
                if (quadChild != null)
                {
                    Debug.Log("Quad child found for button: " + buttons[i].name);
                    Renderer quadRenderer = quadChild.GetComponent<Renderer>();
                    if (quadRenderer != null)
                    {
                        quadRenderer.material.color = colours[i] * 2f;
                    }
                }
            }
        }
        StoreRandomColours();
    }
    public void StoreRandomColours()
    {
        Debug.Log("Storing Random Colours");
        shuffledColours = (Color[])colours.Clone();

        System.Random rng = new System.Random();
        int n = shuffledColours.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Color temp = shuffledColours[k];
            shuffledColours[k] = shuffledColours[n];
            shuffledColours[n] = temp;
        }
        DisplaySequence();
    }

    public void DisplaySequence()
    {
        Debug.Log("Displaying Simon Sequence");
        StartCoroutine(DisplaySequenceCoroutine());
    }

    private System.Collections.IEnumerator DisplaySequenceCoroutine()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < buttons.Length; i++)
        {
            Debug.Log("Simon shows color: " + shuffledColours[i]);
            simonRenderer.material.color = shuffledColours[i] * 2f;
            yield return new WaitForSeconds(3f);
        }

        simonRenderer.material.color = Color.black;
    }

    public void ClearSelectedTags()
    {
        for (int i = 0; i < selectedTags.Length; i++)
        {
            selectedTags[i] = "";
        }
    }
    public void EvaluateAnswer()
    {
        Debug.Log("Evaluating Answer");

        for (int i = 0; i < selectedTags.Length; i++)
        {
            string expectedTag = colorToButtonTag[shuffledColours[i]];
            if (selectedTags[i] != expectedTag)
            {
                Debug.Log("Incorrect sequence. Try again.");
                
                StartSimon();
                return;
            }
        }
        Debug.Log("Correct sequence!");
        simonRenderer.material.color = Color.white;
    }
}