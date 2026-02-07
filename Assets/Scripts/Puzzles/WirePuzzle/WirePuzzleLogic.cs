using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WirePuzzleLogic : MonoBehaviour
{
    Renderer objectRenderer;
    Renderer childRenderer;
    
    public Material matOnAccept; // green
    public Material matTarget;      // red, the targeted light
    public Material matOff;         // Default black of off light
    public int lightMatIndex = 0; // which slot to replace if the mesh has multiple materials

    private bool started = false;
    
    // Assign these in the Inspector: 4 light renderers, 4 button renderers
    [Header("Assign Button Renderers by Button Tag Order (Button1-Button4)")]
    public Renderer[] buttonRenderers = new Renderer[4];
    [Header("Assign Light Renderers by Light Tag Order (Light1 - Light4)")]
    public Renderer[] lightRenderers = new Renderer[4];
    

    // State
    private string[] buttonOrder = new string[4];
    private int selectCounter = 0;
    private bool solved = false;
    
    public AudioSource lightOnSF;
    public AudioSource puzzleFailSF;
    
    public GameFlow gameFlow;
    public Dictionary<int, string> NumToButtonTag = new Dictionary<int, string>
    {
        {1, "Button1"},
        {2, "Button2"},
        {3, "Button3"},
        {4, "Button4"},
    };
    
    // Button tag → light index (array position)
    public Dictionary<string, int> ButtonToLightIndex = new Dictionary<string, int>
    {
        {"Button1", 0},
        {"Button2", 1},
        {"Button3", 2},
        {"Button4", 3},
    };

    void Start()
    {
        bool allButtonsFound = true;
        bool allLightsFound = true;
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
        if (allButtonsFound && allLightsFound)
        {
            Debug.Log("All buttons and lights found.");
        }
        else
        {
            Debug.LogError("One or more buttons/lights not found!");
        }

        if (matOnAccept == null || matOff == null)
        {
            Debug.LogError("Assign matOnAccept and matOff in the Inspector.");
        }

        //StartPuzzle();
        started = true;
    }
    
    public void StartPuzzle()
    {
        Debug.Log("Starting/Restarting puzzle");
        
        // Fresh state
        CreateButtonOrder();
        selectCounter = 0;
        solved = false;
        // Reset indicators
        SetAllLightsTo(matOff);

        // Show the first target as red (only if we actually have an order)
        if (!string.IsNullOrEmpty(buttonOrder[0]))
        {
            int nextLightIndex = ButtonToLightIndex[buttonOrder[0]];
            SetLightTo(nextLightIndex, matTarget);
        }
        
    }

    public void OnButtonPressed(string buttonTag)
    {
        if (started)
        {
            if (solved)
            {
                Debug.Log("Puzzle already solved, ignoring button press");
                return;
            }

            // Not started? Start now and ignore this press (or handle as you prefer).
            if (buttonOrder == null || buttonOrder.Length == 0 || string.IsNullOrEmpty(buttonOrder[0]))
            {
                Debug.LogWarning("Puzzle not initialised yet.");
                //StartPuzzle();
            }

            Debug.Log($"Pressed: {buttonTag} | Expected: {buttonOrder[selectCounter]} (step {selectCounter})");

            bool correct = buttonTag == buttonOrder[selectCounter];

            if (!correct)
            {
                if (puzzleFailSF != null)
                {
                    Debug.Log("Playing Puzzle Fail Sound.");
                    puzzleFailSF.Play();
                }

                Debug.Log("Incorrect press. Resetting puzzle.");
                StartPuzzle();
                return;
            }



            // Correct press: paint THIS light green
            int justPressedLightIndex = ButtonToLightIndex[buttonTag];
            SetLightTo(justPressedLightIndex, matOnAccept);
            selectCounter++;

            if (lightOnSF != null)
            {
                // Stop any current sound on this source if needed, then play the new one
                Debug.Log("Playing Light On Sound.");
                lightOnSF.Play();
            }

            // If that was the last in the sequence, we’re done
            if (selectCounter == buttonOrder.Length)
            {
                solved = true;
                Debug.Log("Puzzle solved! All lights green.");
                gameFlow.WirePuzzleSolved();
                return;
            }

            // Otherwise, mark the NEXT target as red
            int nextLightIndex2 = ButtonToLightIndex[buttonOrder[selectCounter]];
            SetLightTo(nextLightIndex2, matTarget);
        }
        else
        {
            Debug.Log("Puzzle not started yet, ignoring button press");
        }
    }
    
    private void SetAllLightsTo(Material mat)
    {
        if (lightRenderers == null)
        {
            Debug.LogError("lightRenderers is null in SetAllLightsTo");
            return;
        }

        foreach (var rend in lightRenderers)
            SetRendererMaterial(rend, mat);
    }

    private void SetLightTo(int lightIndex, Material mat)
    {
        if (lightIndex < 0 || lightIndex >= lightRenderers.Length) return;
        SetRendererMaterial(lightRenderers[lightIndex], mat);
    }
    
    private void SetRendererMaterial(Renderer rend, Material mat)
    {
        if (rend == null || mat == null) return;

        // Returns a copy of materials array that must be assigned back.
        var mats = rend.materials;
        int idx = Mathf.Clamp(lightMatIndex, 0, mats.Length - 1);
        mats[idx] = mat;
        rend.materials = mats;
        
    }
    
    public void CreateButtonOrder()
    {
        // Create a random order of 1 - 4, then convert to tags
        List<int> randOrder = new List<int> { 1, 2, 3, 4 };

        // Shuffle numbers
        for (int i = randOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1); // UnityEngine.Random
            (randOrder[i], randOrder[j]) = (randOrder[j], randOrder[i]); // tuple swap
        }
        
        // Fill up buttonOrder with tags
        for (int i = 0; i < 4; i++)
        {
            buttonOrder[i] = NumToButtonTag[randOrder[i]]; 
        }
        // Print out the order of the buttons to debug log
        Debug.Log("CBO - Buttons order created:" + string.Join(",", buttonOrder));
    }
}
