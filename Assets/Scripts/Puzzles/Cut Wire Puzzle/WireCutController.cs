using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
public class WireCutController : MonoBehaviour
{
    string[] wireTags = new string[] { "RedWire", "GreenWire", "BlueWire", "YellowWire" };
    string[] shuffledWireTags;
    public string[] selectedTags = new string[4];
    private float timeRemaining = 30f;
    private bool timerIsRunning = false;

    public Renderer AlertLight;
    public Material[] materials = new Material[3]; // 0 = off, 1 = on, 2 = error
    public WireCut[] wireCutComponents;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI wireOrderText;
    public int wireCutAmount = 0;
    public AudioSource BombSFX;
    public AudioSource TimerSFX;
    public GameFlow gameFlow;

    public Dictionary<Color, string> colorToButtonTag = new Dictionary<Color, string>
    {
        { Color.red, "RedWire" },
        { Color.green, "GreenWire" },
        { Color.blue, "BlueWire" },
        { Color.yellow, "YellowWire" }
    };

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                OnTimerExpired();
            }
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void OnTimerExpired()
    {
        if (BombSFX == null)
        {
            Debug.Log("Bomb SFX is null.");
        }
        else
        {
            Debug.Log("Playing bomb SFX.");
            BombSFX.Play();
        }
        Debug.Log("Timer expired!");
        timerText.text = "00:00";
        ChangeAlertLight(2);
    }

    public void OnButtonSelected(string wireTag)
    {
        Debug.Log("Wire with tag " + wireTag + " was cut.");



        for (int i = 0; i < selectedTags.Length; i++)
        {
            if (string.IsNullOrEmpty(selectedTags[i]))
            {
                Debug.Log("Storing tag at position " + i);
                selectedTags[i] = wireTag;
                break;
            }
        }
        wireCutAmount++;
        EvaluateAnswer();
    }

    public void StartWireCut()
    {
        Debug.Log("Wire Cut Started");
        ResetAllWires();
        wireOrderText.text = "Cut the wires in this order:\n";
        wireCutAmount = 0;
        ClearSelectedTags();
        StoreRandomWireTags();
        ChangeAlertLight(1);
        timeRemaining = 30f;
        timerIsRunning = true;
        StartCoroutine(PlayTimerSFX());
        UpdateTimerDisplay();
    }
    public void StoreRandomWireTags()
    {
        Debug.Log("Storing Random Wire Tags");
        shuffledWireTags = (string[])wireTags.Clone();

        System.Random rng = new System.Random();
        int n = shuffledWireTags.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string temp = shuffledWireTags[k];
            shuffledWireTags[k] = shuffledWireTags[n];
            shuffledWireTags[n] = temp;
        }
        DisplaySequence();
    }

    public void DisplaySequence()
    {
        Debug.Log("Displaying Wire Cut Sequence:");
        wireOrderText.text = "";
        for (int i = 0; i < shuffledWireTags.Length; i++)
        {
            
            Debug.Log((i + 1) + ": " + shuffledWireTags[i]);
            wireOrderText.text += (i + 1) + ": " + shuffledWireTags[i] + "\n";
        }
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

        for (int i = 0; i < wireCutAmount; i++)
        {
            if (selectedTags[i] != shuffledWireTags[i])
            {
                Debug.Log("Incorrect sequence. Try again.");
                ChangeAlertLight(2);
                if (BombSFX == null)
                {
                    Debug.Log("Bomb SFX is null.");
                }
                else
                {
                    Debug.Log("Playing bomb SFX.");
                    BombSFX.Play();
                }
                return;
            }
        }
        if (wireCutAmount < shuffledWireTags.Length)
        {
            Debug.Log("Correct so far. Continue cutting.");
        }
        else
        {
            Debug.Log("Correct sequence!");
            timerIsRunning = false;
            wireOrderText.text = "Puzzle Completed!";
            ChangeAlertLight(0);
            gameFlow.CutPuzzleSolved();
        }
    }
    public void ChangeAlertLight(int state)
    {
        Debug.Log("Changing Alert Light Color");
        AlertLight.material = materials[state];
    }

    public void ResetAllWires()
    {
        Debug.Log("Resetting all wires");
        
        if (wireCutComponents != null)
        {
            foreach (WireCut wire in wireCutComponents)
            {
                if (wire != null)
                {
                    wire.ResetWire();
                }
            }
        }
    }

    private IEnumerator PlayTimerSFX()
    {
        if (TimerSFX != null)
        {
            while (timerIsRunning)
            {
                TimerSFX.Play();
                yield return new WaitForSeconds(1.0f);
            }
        }
        
    }
}