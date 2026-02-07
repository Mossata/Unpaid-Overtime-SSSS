using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameFlow : MonoBehaviour
{
    public LightingLogic lightingLogic;
    public WirePuzzleLogic wirePuzzleLogic;
    public FinalWirePuzzleLogic finalWirePuzzleLogic;
    public SimonLogic simonLogic;
    public WireCutController cutPuzzleLogic;
    public ObjectSpawner objectSpawner;
    public MonsterAgent monsterAgent;
    public MonsterBehaviour monsterBehaviour;
    public PuzzleDoorBehaviour puzzleDoorBehaviour;
    public PeepersBehaviour peepersBehaviour;
    public LogManager logManager;
    private bool wirePuzzleCompleted = false;
    private bool simonPuzzleCompleted = false;
    private bool cutPuzzleCompleted = false;
    private bool peepersEventCompleted = false;
    private bool finalWirePuzzleCompleted = false;

    public bool wirePuzzleActive = false;
    public bool simonPuzzleActive = false;
    public bool cutPuzzleActive = false;
    public bool peepersEventActive = false;
    public bool finalWirePuzzleActive = false;

    public float wirePuzzleTimer = 0f;
    public float simonPuzzleTimer = 0f;
    public float cutPuzzleTimer = 0f;
    public float peepersEventTimer = 0f;
    public float finalWirePuzzleTimer = 0f;

    void Start()
    {
        if (lightingLogic == null || wirePuzzleLogic == null || simonLogic == null || cutPuzzleLogic == null || peepersBehaviour == null || finalWirePuzzleLogic == null)
        {
            Debug.LogError("Assign all puzzle logic scripts in the Inspector.");
            return;
        }

        lightingLogic.StartGame();
    }
    void Update()
    {
        //Debug.Log("Timescale = " + Time.timeScale);
        if (wirePuzzleActive && !wirePuzzleCompleted)
        {
            wirePuzzleTimer += Time.deltaTime;
        }
        if (simonPuzzleActive && !simonPuzzleCompleted)
        {
            simonPuzzleTimer += Time.deltaTime;
        }
        if (cutPuzzleActive && !cutPuzzleCompleted)
        {
            cutPuzzleTimer += Time.deltaTime;
        }
        if (peepersEventActive && !peepersEventCompleted)
        {
            peepersEventTimer += Time.deltaTime;
        }
        if (finalWirePuzzleActive && !finalWirePuzzleCompleted)
        {
            finalWirePuzzleTimer += Time.deltaTime;
        }
    }

    public void StartWirePuzzle()
    {
        wirePuzzleActive = true;
        wirePuzzleLogic.StartPuzzle();
    }

    public void WirePuzzleSolved()
    {
        Debug.Log("Wire Puzzle Solved!");
        objectSpawner.RespawnObjects();
        if (monsterBehaviour.isAgentControlled)
        {
            monsterAgent.HandlePuzzleSolved(-0.01f);
        }
        logManager.firstHoleWhite();
        wirePuzzleCompleted = true;
        lightingLogic.EndWirePuzzle();
    }
    public void SimonPuzzleSolved()
    {
        Debug.Log("Simon Puzzle Solved!");
        objectSpawner.RespawnObjects();
        if (monsterBehaviour.isAgentControlled)
        {
            monsterAgent.HandlePuzzleSolved(-0.01f);
        }
        simonPuzzleCompleted = true;
        lightingLogic.EndSimonPuzzle();
    }
    public void PeeperEventSolved()
    {
        Debug.Log("Peeper Puzzle Solved!");
        // objectSpawner.RespawnObjects();
        if (monsterBehaviour.isAgentControlled)
        {
            monsterAgent.HandlePuzzleSolved(-0.01f);
        }
        lightingLogic.EndPeeperEvent();
        peepersEventCompleted = true;
        cutPuzzleActive = true;
        StartCoroutine(puzzleDoorBehaviour.OpenDoor(false));
    }
    public void CutPuzzleSolved()
    {
        Debug.Log("Cut Puzzle Solved!");
        if (monsterBehaviour.isAgentControlled)
        {
            monsterAgent.HandlePuzzleSolved(-0.01f);
        }
        cutPuzzleCompleted = true;
        StartCoroutine(puzzleDoorBehaviour.CloseDoor(true));
        finalWirePuzzleActive = true;
        lightingLogic.StartFinalPuzzle();
    }

    public void SkipToFinalPuzzle()
    {
        wirePuzzleCompleted = true;
        simonPuzzleCompleted = true;
        cutPuzzleCompleted = true;
        peepersEventCompleted = true;
        StartCoroutine(puzzleDoorBehaviour.FinalPuzzleSkip());
        finalWirePuzzleActive = true;
        lightingLogic.StartFinalPuzzle();
    }

    public void FinalWirePuzzleSolved()
    {
        Debug.Log("Final Wire Puzzle Solved!");
        if (monsterBehaviour.isAgentControlled)
        {
            monsterAgent.HandlePuzzleSolved(-0.01f);
        }
        finalWirePuzzleCompleted = true;
        lightingLogic.EndFinalPuzzle();
        StartCoroutine(puzzleDoorBehaviour.CloseDoor(false));
        objectSpawner.RespawnObjects();
    }
    public void CellInserted()
    {
        Debug.Log("Cell Inserted!");
        if (wirePuzzleCompleted && !simonPuzzleCompleted && !peepersEventCompleted && !cutPuzzleCompleted)
        {
            StartCoroutine(SimonStarting());
            simonPuzzleActive = true;
        }
        else if (wirePuzzleCompleted && simonPuzzleCompleted && !peepersEventCompleted && !cutPuzzleCompleted)
        {
            peepersEventActive = true;
            lightingLogic.StartPeeperEvent();
        }
        else if (wirePuzzleCompleted && simonPuzzleCompleted && peepersEventCompleted && cutPuzzleCompleted && finalWirePuzzleCompleted)
        {

            lightingLogic.GameWin();

        }
        else
        {
            Debug.Log("What? How did you get this cell? Cheater!");
        }
    }
    public void GameWin()
    {
        StartCoroutine(GameWinSequence());
        if (monsterBehaviour.isAgentControlled)
        {
            monsterAgent.HandlePlayerWin(-0.05f);
        }
    }
    private IEnumerator GameWinSequence()
    {
        yield return new WaitForSeconds(2.0f);
        //SceneManager.LoadScene("StartScene");
    }
    public void PeeperStart()
    {
        peepersBehaviour.DebugButtonPress();
    }

    private IEnumerator SimonStarting()
    {
        yield return new WaitForSeconds(2.0f);
        simonLogic.StartSimon();
        lightingLogic.StartSimonPuzzle();
    }
}