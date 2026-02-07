using System.Collections.Generic;
using UnityEngine;

public class SimonLogic : MonoBehaviour
{
    public Material[] mats = new Material[10];
    public Material matAccept;
    public Material matReject;
    public Material matOff;
    public GameObject replayButton;
    private int buttonMatIndex = 0; // which slot to replace if the mesh has multiple materials
    
    // Assign these in the Inspector: 9 button renderers, 9 dotlight renderers
    [Header("Assign Button Renderers by Button Tag Order (Button1-Button9)")]
    public Renderer[] buttonRenderers = new Renderer[9];
    [Header("Assign Dotlight Renderers by Index (0-8)")]
    public Renderer[] dotlightRenderers = new Renderer[9];

    // State
    private bool buttonsActive = false;
    private string[] buttonOrder = new string[9];
    private string[] selectedTags = new string[9];
    private int selectCounter = 0;

    private int stageNumber = 0;
    public int advancementAmount;
    public AudioSource simonSF;
    public AudioSource failSF;
    public AudioSource startSF;
    public AudioSource winSF;
    public GameFlow gameFlow;

    public Dictionary<int, string> NumToButtonTag = new Dictionary<int, string>
    {
        {1, "Button1"},
        {2, "Button2"},
        {3, "Button3"},
        {4, "Button4"},
        {5, "Button5"},
        {6, "Button6"},
        {7, "Button7"},
        {8, "Button8"},
        {9, "Button9"}
    };

    public Dictionary<string, int> ButtonTagToNum = new Dictionary<string, int>
    {
        {"Button1", 1},
        {"Button2", 2},
        {"Button3", 3},
        {"Button4", 4},
        {"Button5", 5},
        {"Button6", 6},
        {"Button7", 7},
        {"Button8", 8},
        {"Button9", 9}
    };
    
    public void Start()
    {
        // Optionally, validate assignments
        if (buttonRenderers.Length != 9)
        {
            Debug.LogError("buttonRenderers array must have 9 elements (Button1-Button9)");
        }
        if (dotlightRenderers.Length != 9)
        {
            Debug.LogError("dotlightRenderers array must have 9 elements (0-8)");
        }
    }
    public void OnButtonSelected(string buttonTag)
    {
        Debug.Log("OnButtonSelected called with tag: " + buttonTag);
        if (buttonTag == "Replay")
        {
            DisplayAgain();
            return;
        }
        if (buttonsActive)
        {
            // Flash button to show confirmation 
            StartCoroutine(FlashButtonCoroutine(buttonTag));
            
            Debug.Log("Button with tag " + buttonTag + " was selected.");

            for (int i = 0; i < selectedTags.Length; i++)
            {
                if (string.IsNullOrEmpty(selectedTags[i]))
                {
                    Debug.Log("Storing tag at position " + i);
                    selectedTags[i] = buttonTag;
                    selectCounter++;
                    break;
                }
            }
            if (selectCounter >= stageNumber)
            {
                Debug.Log("Enough buttons have been pressed for this stage: " + string.Join(", ", selectedTags));
                EvaluateAnswer();
            }
        }
        else
        {
            Debug.Log("Button " + buttonTag + " was pressed but buttons are not active.");
        }
    }

    public void StartSimon()
    {
        // Starting or Restarting the simon says sequence
        Debug.Log("Starting Simon Sequence");
        CreateButtonOrder();
        stageNumber = 0;
        if (startSF != null)
        {
            // Play start puzzle noise.
            startSF.Play();
        }
        AdvanceStage();
        
    }

    public void AdvanceStage()
    {
        buttonsActive = false;
        // Move to next stage
        stageNumber = stageNumber + advancementAmount;
        if (stageNumber < 10)
        {
            // Fresh state
            ClearSelectedTags();
            selectCounter = 0;

            Debug.Log($"Moving to stage {stageNumber}");
            StartCoroutine(AdvanceDotLights(stageNumber));
            DisplaySequence();
        }
        else
        {
            // All stage must have been completed successfully
            StartCoroutine(ShowVictoryDotLights());
            gameFlow.SimonPuzzleSolved();
            buttonsActive = false;
        }
    }
    public void DebugWin()
    {
        gameFlow.SimonPuzzleSolved();
    }
    
    public void CreateButtonOrder()
    {
        // Create a random order of 1 - 9, then convert to tags
        List<int> randOrder = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // Shuffle numbers
        for (int i = randOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1); // UnityEngine.Random
            (randOrder[i], randOrder[j]) = (randOrder[j], randOrder[i]); // tuple swap
        }

        // Fill up buttonOrder with tags
        for (int i = 0; i < 9; i++)
        {
            buttonOrder[i] = NumToButtonTag[randOrder[i]];
        }
        // Print out the order of the buttons to debug log
        Debug.Log("CBO - Buttons order created:" + string.Join(",", buttonOrder));
    }

    public void DisplaySequence()
    {
        Debug.Log($"Displaying Simon Sequence for Stage {stageNumber}");
        StartCoroutine(DisplaySequenceCoroutine(stageNumber));
    }

    public void DisplayAgain()
    {
        if (buttonsActive)
        {
            buttonsActive = false;
            Debug.Log("Displaying Simon Sequence Again for Stage " + stageNumber);
            ClearSelectedTags();
            selectCounter = 0;
            DisplaySequence();
        }
    }
    private System.Collections.IEnumerator DisplaySequenceCoroutine(int stageNum)
    {
        // Give the 'Button flash' time to finish first 
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < stageNum; i++)
        {
            string currentButtonTag = buttonOrder[i];
            //Debug.Log("Lighting up button: " + currentButtonTag);
            if (simonSF != null)
            {
                Debug.Log("Playing flashing light sound");
                simonSF.Play();
            }
            SetButtonTo(currentButtonTag, mats[ButtonTagToNum[currentButtonTag] - 1]);
            // Wait for 2 seconds
            yield return new WaitForSeconds(1f);
            SetButtonTo(currentButtonTag, mats[9]);
        }
        buttonsActive = true;
    }

    private System.Collections.IEnumerator FlashButtonCoroutine(string buttonToFlashTag)
    {
        SetButtonTo(buttonToFlashTag, mats[ButtonTagToNum[buttonToFlashTag] - 1]);
        // Wait for 2 seconds
        yield return new WaitForSeconds(0.25f);
        SetButtonTo (buttonToFlashTag, mats[9]);
    }

    private System.Collections.IEnumerator AdvanceDotLights(int stageNum)
    {
        for (int i = 0; i < 2; i++)
        {
            // Flash stage dot on and off 3 times
            SetRendererMaterial(dotlightRenderers[stageNum - 1], matAccept);
            yield return new WaitForSeconds(0.4f);
            SetRendererMaterial(dotlightRenderers[stageNum - 1], matOff);
            yield return new WaitForSeconds(0.4f);
        }
        SetRendererMaterial(dotlightRenderers[stageNum - 1], matAccept);
    }

    public System.Collections.IEnumerator ShowRejectionDotLights()
    {
        //Flash all dots on and off 3 times
        for (int i = 0; i < 3; i++)
        {
            if (failSF != null)
            {
                Debug.Log("Playing Puzzle Fail Sound.");
                failSF.Play();
            }
            for (int j = 0; j < 9; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matReject);
            }
            yield return new WaitForSeconds(0.4f);
            for (int j = 0; j < 9; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matOff);
            }
            yield return new WaitForSeconds(0.4f);
        }
        StartSimon();
    }
    
    private System.Collections.IEnumerator ShowVictoryDotLights() 
    {
        //Flash all dots on and off 3 times
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matOff);
            }
            yield return new WaitForSeconds(0.4f);
            for (int j = 0; j < 9; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matAccept);
            }
            yield return new WaitForSeconds(0.4f);
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
        bool correctAnswer = true;
        
        for (int i = 0; i < stageNumber; i++)
        {
            if (selectedTags[i] != buttonOrder[i])
            {
                correctAnswer = false;
            }
        }

        if (correctAnswer)
        {
            Debug.Log("Correct sequence!");
            if (winSF != null)
            {
                winSF.Play();
            }
            AdvanceStage();
        }
        else
        {
            Debug.Log("Incorrect sequence. Try again.");
            StartCoroutine(ShowRejectionDotLights());
        }
    }
    
    private void SetButtonTo(string buttonTag, Material mat)
    {
        int idx = ButtonTagToNum.ContainsKey(buttonTag) ? ButtonTagToNum[buttonTag] - 1 : -1;
        if (idx < 0 || idx >= buttonRenderers.Length)
        {
            Debug.LogError($"Invalid buttonTag {buttonTag} in SetButtonTo");
            return;
        }
        var rend = buttonRenderers[idx];
        if (rend == null)
        {
            Debug.LogError($"Renderer for {buttonTag} is null in SetButtonTo");
            return;
        }
        if (mat == null)
        {
            Debug.LogError($"Material is null in SetButtonTo for tag {buttonTag}");
            return;
        }
        Debug.Log("Setting button " + buttonTag + " to material " + mat.name);
        SetRendererMaterial(rend, mat);
    }
    
    private void SetRendererMaterial(Renderer rend, Material mat)
    {
        if (rend == null || mat == null) return;
        var maters = rend.materials;
        int idx = Mathf.Clamp(buttonMatIndex, 0, maters.Length - 1);
        maters[idx] = mat;
        rend.materials = maters;
    }
}