using UnityEngine;
using System.Collections.Generic;
using URandom = UnityEngine.Random;
using SRandom = System.Random;


public class FinalWirePuzzleLogic : MonoBehaviour
{
    public ObjectSpawner objectSpawner;
    
    public Material[] mats = new Material[10];
    public Material matAccept; // green
    public Material matReject;      // red, the targeted light
    public Material matOff;         // Default black of off light
    public Material matButton;
    private int lightMatIndex = 0; // which slot to replace if the mesh has multiple materials
    
    // State
    private string[] buttonOrder = new string[4];
    private bool buttonsActive = true;
    private string[] selectedTags = new string[4];
    private int selectCounter = 0;
    
    private int stageNumber = 0;
    public int advancementAmount;
    private Coroutine currentSequenceCoroutine;
    
    public AudioSource simonSF;
    public AudioSource failSF;
    public AudioSource winSF;
    
    public GameFlow gameFlow;
    public Dictionary<int, string> NumToButtonTag = new Dictionary<int, string>
    {
        {1, "Button1"},
        {2, "Button2"},
        {3, "Button3"},
        {4, "Button4"}
    };
    
    public Dictionary<string, int> ButtonTagToNum = new Dictionary<string, int>
    {
        {"Button1", 1},
        {"Button2", 2},
        {"Button3", 3},
        {"Button4", 4}
    };
    
    public Dictionary<string, string> ButtonToLightTag = new Dictionary<string, string>
    {
        {"Button1", "Light1"},
        {"Button2", "Light2"},
        {"Button3", "Light3"},
        {"Button4", "Light4"},
    };
    
    public Dictionary<string, int> LightTagToNum = new Dictionary<string, int>
    {
        {"Light1", 1},
        {"Light2", 2},
        {"Light3", 3},
        {"Light4", 4}
    };
    
    // Assign these in the Inspector: 4 light renderers, 4 dotlight renderers, 4 button renderers
    [Header("Assign Light Renderers by Light Tag Order (Light1 - Light4)")]
    public Renderer[] lightRenderers = new Renderer[4];
    [Header("Assign Button Renderers by Button Tag Order (Button1-Button4)")]
    public Renderer[] buttonRenderers = new Renderer[4];
    [Header("Assign Dotlight Renderers by Index (0-4)")]
    public Renderer[] dotlightRenderers = new Renderer[4];
    public Renderer colourDisplay;

    void Start()
    {
        bool allButtonsFound = true;
        bool allLightsFound = true;
        bool allDotLightsFound = true;
        foreach (Renderer butren in buttonRenderers)
        {
            if (butren == null)
            {
                allButtonsFound = false;
                break;
            }
        }

        foreach (Renderer lightren in lightRenderers)
        {
            if (lightren == null)
            {
                allLightsFound = false;
                break;
            }
        }
        foreach (Renderer dotlightren in dotlightRenderers)
        {
            if (dotlightren == null)
            {
                allDotLightsFound = false;
                break;
            }
        }
        if (allButtonsFound && allLightsFound && allDotLightsFound)
        {
            Debug.Log("All buttons and lights found.");
        }
        else
        {
            Debug.LogError("One or more buttons/lights not found!");
        }

        Debug.Log("All buttons and lights found.");
        ShuffleMats();
    }
    
    public void OnButtonSelected(string buttonTag)
    {
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

    public void StartFinalPuzzle()
    {
        // Starting or Restarting the puzzle sequence
        Debug.Log("Starting Final Puzzle Sequence");
        CreateButtonOrder();
        stageNumber = 0;
        AdvanceStage();
    }

    public void AdvanceStage()
    {
        //buttonsActive = false;
        // Move to next stage
        stageNumber = stageNumber + advancementAmount;
        if (stageNumber < 5)
        {
            // Fresh state
            ClearSelectedTags();
            selectCounter = 0;

            Debug.Log($"Moving to stage {stageNumber}");
            StartCoroutine(AdvanceDotLights(stageNumber));
            if (currentSequenceCoroutine != null)
            {
                StopCoroutine(currentSequenceCoroutine);
            }
            DisplaySequenceUpTo(stageNumber);
        }
        else
        {
            // All stage must have been completed successfully
            StopCoroutine(currentSequenceCoroutine);
            StartCoroutine(ShowVictoryDotLights());
        }
    }

    public void ShuffleMats()
    {
        SRandom numGen = new SRandom();
        int num = mats.Length;
        while (num > 1)
        {
            num--;
            int k = numGen.Next(num + 1); // 0 ≤ k ≤ length of array
            (mats[k], mats[num]) = (mats[num], mats[k]);
        }
    }
    public void CreateButtonOrder()
    {
        // Create a random order of 1 - 4, then convert to tags
        List<int> randOrder = new List<int> { 1, 2, 3, 4 };

        // Shuffle numbers
        for (int i = randOrder.Count - 1; i > 0; i--)
        {
            int j = URandom.Range(0, i + 1); // UnityEngine.Random
            (randOrder[i], randOrder[j]) = (randOrder[j], randOrder[i]); // tuple swap
        }
        
        // Fill up buttonOrder with tags
        for (int i = 0; i < 4; i++)
        {
            buttonOrder[i] = NumToButtonTag[randOrder[i]]; 
        }
        // Print out the order of the buttons to debug log
        Debug.Log("CBO - Buttons order created:" + string.Join(",", buttonOrder));
        
        // Set circuit lights to correct colours
        foreach (string buttonTag in buttonOrder)
        {
            string eqLightTag = ButtonToLightTag[buttonTag];
            int eqLightNum = LightTagToNum[eqLightTag];
            SetRendererMaterial(lightRenderers[eqLightNum - 1], mats[ButtonTagToNum[buttonTag] - 1]);
        }
    }
    public void DisplaySequenceUpTo(int stageNum)
    {
        Debug.Log($"Displaying Sequence for Stage {stageNum}");
        currentSequenceCoroutine = StartCoroutine(DisplaySequenceCoroutine(stageNum));
    }
    
    private System.Collections.IEnumerator DisplaySequenceCoroutine(int stageNum)
    {
        // Have a break between loops
        yield return new WaitForSeconds(5f);
    
        for (int i = 0; i < stageNum; i++)
        {
            if (simonSF != null)
            {
                Debug.Log("Playing colour flash sound.");
                simonSF.Play();
            }
            string buttonToFlashTag = buttonOrder[i]; 
            SetRendererMaterial(colourDisplay, mats[ButtonTagToNum[buttonToFlashTag] - 1]);
            // Wait for a second
            yield return new WaitForSeconds(1f);
        }
        SetRendererMaterial(colourDisplay, matOff);
        DisplaySequenceUpTo(stageNum);
        //buttonsActive = true;
    }

    private System.Collections.IEnumerator FlashButtonCoroutine(string buttonToFlashTag)
    {
        SetButtonTo(buttonToFlashTag, mats[ButtonTagToNum[buttonToFlashTag] - 1]);
        // Wait for 2 seconds
        yield return new WaitForSeconds(0.25f);
        SetButtonTo (buttonToFlashTag, matButton);
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

    private System.Collections.IEnumerator ShowRejectionDotLights()
    {
        //Flash all dots on and off 3 times
        for (int i = 0; i < 3; i++)
        {
            if (failSF != null)
            {
                Debug.Log("Playing Puzzle Fail Sound.");
                failSF.Play();
            }
            for (int j = 0; j < 4; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matReject);
            }
            yield return new WaitForSeconds(0.4f);
            for (int j = 0; j < 4; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matOff);
            }
            yield return new WaitForSeconds(0.4f);
        }
        StartFinalPuzzle();
    }
    
    private System.Collections.IEnumerator ShowVictoryDotLights() 
    {
        //Flash all dots on and off 3 times
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matOff);
            }
            yield return new WaitForSeconds(0.4f);
            for (int j = 0; j < 4; j++)
            {
                SetRendererMaterial(dotlightRenderers[j], matAccept);
            }
            yield return new WaitForSeconds(0.4f);
        }
        gameFlow.FinalWirePuzzleSolved();
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
        int idx = Mathf.Clamp(lightMatIndex, 0, maters.Length - 1);
        maters[idx] = mat;
        rend.materials = maters;
    }
    
    
}
