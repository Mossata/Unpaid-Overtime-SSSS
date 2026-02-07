using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoPlayer : MonoBehaviour
{
    [Header("References")]
    public PlayerFOVRaycast playerFOV;
    public GameFlow gameFlow;
    public LogManager logManager;
    public Transform playerHead;
    public MonsterBehaviour monsterBehaviour;
    public WirePuzzleLogic wirePuzzleLogic;
    public SimonLogic simonLogic;
    public Transform[] hitLocArr; // Array of hit location transforms
    
    [Header("Bot Skill Settings")]
    [Tooltip("How quickly bot turns to look at monsters (higher = faster reaction)")]
    public float lookSpeed = 10f;
    
    [Tooltip("Delay before solving puzzle components (lower = faster puzzle solving)")]
    public float puzzleSolveDelay = 8.0f;
    
    [Tooltip("Chance per frame to look away from monster (creates realistic behavior)")]
    [Range(0f, 0.1f)]
    public float lookAwayChance = 0.02f;
    
    [Tooltip("How long bot looks at monster before potentially looking away")]
    public float focusTime = 2f;
    
    [Header("Training Modes")]
    public bool trainingMode = true;
    public TrainingDifficulty difficulty = TrainingDifficulty.Easy;
    
    [Header("Current Difficulty Values (Runtime)")]
    [SerializeField, Tooltip("Current look speed (set by difficulty)")]
    private float currentLookSpeed;
    [SerializeField, Tooltip("Current puzzle solve delay (set by difficulty)")]
    private float currentPuzzleSolveDelay;
    [SerializeField, Tooltip("Current look away chance (set by difficulty)")]
    private float currentLookAwayChance;
    [SerializeField, Tooltip("Current focus time (set by difficulty)")]
    private float currentFocusTime;
    
    private bool isLookingAtMonster = false;
    private float lookStartTime = 0f;
    private Coroutine puzzleSolveCoroutine;
    
    // Monster position tracking
    private int currentMonsterLocationIndex = 0;
    private float lastLocationSwitchTime = 0f;
    private float locationSwitchInterval = 2f; // Switch focus every 2 seconds
    
    public enum TrainingDifficulty
    {
        Easy,      // Bot is very good
        Medium,    // Bot has some delays and mistakes
        Hard,      // Bot is slow and sometimes misses the monster
        Random     // Randomly picks difficulty each episode
    }
    
    void Start()
    {
        if (trainingMode)
        {
            SetDifficultyParameters();
        }
    }
    
    void Update()
    {
        if (!trainingMode) return;
        
        // Main bot behaviors
        AutoLookAtMonsters();
        AutoSolvePuzzles();
        HandleMonsterInteraction();
    }
    
    void SetDifficultyParameters()
    {
        TrainingDifficulty currentDiff = difficulty;
        
        // Randomize difficulty if set to Random
        if (difficulty == TrainingDifficulty.Random)
        {
            currentDiff = (TrainingDifficulty)Random.Range(0, 3);
        }
        
        switch (currentDiff)
        {
            case TrainingDifficulty.Easy:
                lookSpeed = 3f;             // Slower, more realistic movement
                puzzleSolveDelay = 10f;      // Fast enough to avoid monster pressure
                lookAwayChance = 0.003f;    // Very low chance to look away (0.3%)
                focusTime = 4f;             // Safe margin (4s > 2.5s needed)
                break;
                
            case TrainingDifficulty.Medium:
                lookSpeed = 2f;             // Medium movement speed
                puzzleSolveDelay = 13f;      // Creates some monster pressure
                lookAwayChance = 0.01f;     // 1% chance to look away
                focusTime = 3.2f;           // Safe but tighter (3.2s > 2.5s needed)
                break;
                
            case TrainingDifficulty.Hard:
                lookSpeed = 1.5f;           // Slow movement
                puzzleSolveDelay = 15f;     // High pressure - multiple monsters possible
                lookAwayChance = 0.025f;    // 2.5% chance to look away
                focusTime = 2.8f;           // Risky but just safe (2.8s > 2.5s)
                break;
        }
        
        // Update inspector display values
        currentLookSpeed = lookSpeed;
        currentPuzzleSolveDelay = puzzleSolveDelay;
        currentLookAwayChance = lookAwayChance;
        currentFocusTime = focusTime;
        
        Debug.Log($"AutoPlayer set to {currentDiff} difficulty");
    }
    
    void AutoLookAtMonsters()
    {
        GameObject activeMonster = FindActiveMonster();
        int activeMonsterIndex = GetActiveMonsterIndex();
        
        if (activeMonster != null && activeMonsterIndex >= 0 && hitLocArr != null && activeMonsterIndex < hitLocArr.Length)
        {
            // Start looking at monster's hitLoc
            if (!isLookingAtMonster)
            {
                isLookingAtMonster = true;
                lookStartTime = Time.time;
            }
            
            // Calculate look direction to the actual monster
            Vector3 monsterPos = activeMonster.transform.position;
            
            // Look directly at the monster - no adjustments
            Vector3 lookDirection = (monsterPos - playerHead.position).normalized;
            
            // Look in direction
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            playerHead.rotation = Quaternion.Slerp(
                playerHead.rotation, 
                targetRotation, 
                lookSpeed * Time.deltaTime
            );
            
            // Check if we should look away
            float lookDuration = Time.time - lookStartTime;
            
            // Increase look away chance during harder puzzles
            float currentLookAwayChance = lookAwayChance;
            if (gameFlow.simonPuzzleActive)
            {
                currentLookAwayChance *= 1.2f; // 20% more during Simon Says
            }
            else if (gameFlow.peepersEventActive)
            {
                currentLookAwayChance *= 1.5f; // 50% more during Peeper event
            }
            else if (gameFlow.finalWirePuzzleActive)
            {
                currentLookAwayChance *= 1.5f; // 50% more during final puzzle
            }
            
            if (lookDuration > focusTime && Random.value < currentLookAwayChance)
            {
                LookAway();
            }
        }
        else
        {
            isLookingAtMonster = false;
            // Look around like a fool when there's no monster
            RandomLookAround();
        }
    }
    
    GameObject FindActiveMonster()
    {
        if (monsterBehaviour?.monsterArr == null) return null;
        
        foreach (GameObject monster in monsterBehaviour.monsterArr)
        {
            if (monster.activeInHierarchy)
            {
                return monster;
            }
        }
        return null;
    }
    
    int GetActiveMonsterIndex()
    {
        if (monsterBehaviour?.monsterArr == null) return -1;
        
        for (int i = 0; i < monsterBehaviour.monsterArr.Length; i++)
        {
            if (monsterBehaviour.monsterArr[i].activeInHierarchy)
            {
                return i;
            }
        }
        return -1;
    }
    
    void LookAway()
    {
        // Look in a random direction 
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = Mathf.Clamp(randomDirection.y, -0.3f, 0.3f); // Mostly horizontal, gamers dont look up
        
        Quaternion randomRotation = Quaternion.LookRotation(randomDirection);
        StartCoroutine(LookAwayTemporarily(randomRotation));
    }
    
    IEnumerator LookAwayTemporarily(Quaternion targetRotation)
    {
        float lookAwayTime = Random.Range(0.5f, 1.5f);
        float elapsed = 0f;
        
        Quaternion startRotation = playerHead.rotation;
        
        while (elapsed < lookAwayTime)
        {
            playerHead.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / lookAwayTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isLookingAtMonster = false;
    }
    
    void RandomLookAround()
    {
        // Cycle through the 4 monster locations semi-evenly
        if (Time.time - lastLocationSwitchTime > locationSwitchInterval)
        {
            currentMonsterLocationIndex = (currentMonsterLocationIndex + 1) % 4;
            lastLocationSwitchTime = Time.time;
            locationSwitchInterval = Random.Range(2.5f, 4f); // Longer interval to allow full rotation
        }
        
        Vector3 targetDirection = GetMonsterLocationDirection(currentMonsterLocationIndex);
        
        // Add some random variation to make it less robotic
        float randomVariationX = Random.Range(-10f, 10f);
        float randomVariationY = Random.Range(-5f, 5f);
        
        Vector3 randomVariation = Quaternion.Euler(randomVariationX, randomVariationY, 0) * Vector3.forward;
        Vector3 finalDirection = Vector3.Slerp(targetDirection, randomVariation, 0.2f).normalized;
        
        Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
        
        // Increased look speed for random looking to complete full rotations
        playerHead.rotation = Quaternion.Slerp(playerHead.rotation, targetRotation, 4f * Time.deltaTime);
    }
    
    Vector3 GetMonsterLocationDirection(int locationIndex)
    {
        // Use actual hitLoc positions if available, otherwise fall back to relative directions
        if (hitLocArr != null && locationIndex < hitLocArr.Length && hitLocArr[locationIndex] != null)
        {
            Vector3 hitLocPos = hitLocArr[locationIndex].position;
            
            // Look directly at the center of the hitLoc - no adjustments
            return (hitLocPos - playerHead.position).normalized;
        }
        
        // Fallback to relative directions if hitLoc not available
        switch (locationIndex)
        {
            case 0: // Behind player
                return (-playerHead.forward + Vector3.up * 0.1f).normalized;
            case 1: // Forward and to the left
                return (Quaternion.Euler(0, -45f, 0) * playerHead.forward + Vector3.up * 0.1f).normalized;
            case 2: // Directly in front
                return (playerHead.forward + Vector3.up * 0.1f).normalized;
            case 3: // Directly above - ceiling position
                return (playerHead.up + playerHead.forward * 0.2f).normalized; // Look slightly forward and up
            default:
                return (playerHead.forward + Vector3.up * 0.1f).normalized;
        }
    }
    
    void AutoSolvePuzzles()
    {
        // Only start new puzzle solving if not already solving
        if (puzzleSolveCoroutine != null) return;

        // Check which puzzle is active and solve it
        if (gameFlow.wirePuzzleActive)
        {
            puzzleSolveCoroutine = StartCoroutine(SolveWirePuzzle());
        }
        else if (gameFlow.simonPuzzleActive)
        {
            puzzleSolveCoroutine = StartCoroutine(SolveSimonPuzzle());
        }
        else if (gameFlow.cutPuzzleActive)
        {
            puzzleSolveCoroutine = StartCoroutine(SolveCutPuzzle());
        }
        else if (gameFlow.peepersEventActive)
        {
            puzzleSolveCoroutine = StartCoroutine(SolvePeeperPuzzle());
        }
        else if (gameFlow.finalWirePuzzleActive)
        {
            puzzleSolveCoroutine = StartCoroutine(SolveFinalWirePuzzle());
        }
    }

    IEnumerator SolveWirePuzzle()
    {
        yield return new WaitForSeconds(puzzleSolveDelay);

        gameFlow.WirePuzzleSolved();
        gameFlow.wirePuzzleActive = false;
        gameFlow.simonPuzzleActive = true;

        puzzleSolveCoroutine = null;
    }
    IEnumerator SolveCutPuzzle()
    {
        yield return new WaitForSeconds(puzzleSolveDelay);

        gameFlow.CutPuzzleSolved();
        gameFlow.cutPuzzleActive = false;
        gameFlow.finalWirePuzzleActive = true;

        puzzleSolveCoroutine = null;
    }
    IEnumerator SolvePeeperPuzzle()
    {
        yield return new WaitForSeconds(puzzleSolveDelay);

        gameFlow.PeeperEventSolved();
        gameFlow.peepersEventActive = false;
        gameFlow.cutPuzzleActive = true;
        
        puzzleSolveCoroutine = null;
    }
    
    IEnumerator SolveSimonPuzzle()
    {
        yield return new WaitForSeconds(puzzleSolveDelay * 2);

        gameFlow.SimonPuzzleSolved();
        gameFlow.simonPuzzleActive = false;
        gameFlow.peepersEventActive = true;
        
        puzzleSolveCoroutine = null;
    }

    IEnumerator SolveFinalWirePuzzle()
    {
        yield return new WaitForSeconds(puzzleSolveDelay * 2);

        gameFlow.FinalWirePuzzleSolved();
        yield return new WaitForSeconds(puzzleSolveDelay * 2);
        gameFlow.finalWirePuzzleActive = false;
        gameFlow.GameWin();

        puzzleSolveCoroutine = null;
    }
    
    void HandleMonsterInteraction()
    {
        if (monsterBehaviour != null)
        {
            GameObject activeMonster = FindActiveMonster();
            int activeMonsterIndex = GetActiveMonsterIndex();
            
            if (activeMonster != null && activeMonsterIndex >= 0 && isLookingAtMonster && 
                hitLocArr != null && activeMonsterIndex < hitLocArr.Length)
            {
                // Calculate direction to the actual monster instead of hitLoc
                Vector3 monsterPos = activeMonster.transform.position;

                // Look directly at the monster - no adjustments
                Vector3 toMonster = (monsterPos - playerHead.position).normalized;
                float dot = Vector3.Dot(playerHead.forward, toMonster);

                monsterBehaviour.beingWatched = dot > 0.866f; 
                
            }
            else
            {
                monsterBehaviour.beingWatched = false;
            }
        }
    }
    
    public void OnEpisodeBegin()
    {
        if (puzzleSolveCoroutine != null)
        {
            StopCoroutine(puzzleSolveCoroutine);
            puzzleSolveCoroutine = null;
        }
        
        isLookingAtMonster = false;
        
        // Reset monster location tracking
        currentMonsterLocationIndex = Random.Range(0, 4); // Start at random position
        lastLocationSwitchTime = Time.time;
        locationSwitchInterval = Random.Range(1.5f, 3f);
        
        SetDifficultyParameters();
        
        Debug.Log("AutoPlayer reset for new episode");
    }
}