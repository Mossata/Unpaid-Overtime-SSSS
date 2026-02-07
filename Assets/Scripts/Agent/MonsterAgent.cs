using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.MLAgents.Policies;
using Unity.MLAgents.SideChannels;

public class MonsterAgent : Agent
{
    // Add references for puzzle managers and cell objects
    public LogManager logManager;
    public WirePuzzleLogic wirePuzzleLogic;
    public SimonLogic simonLogic;
    public ObjectSpawner objectSpawner;

    public GameObject headCenter;
    public MonsterBehaviour monsterBehaviour;
    public PlayerFOVRaycast playerFOVRaycast;
    public GameFlow gameFlow;

    public int[] monsterLocationHitCounts;
    private float timescale;
    
    // Checkpoint variables
    private int stepCount = 0;
    private int checkpointInterval = 500; // Save every 500 steps
    private string checkpointFilePath;
    
    // TensorBoard logging variables
    private StatsRecorder statsRecorder;
    private int episodeCount = 0;

    // Autoplayer

    public AutoPlayer autoPlayer;

    void Start()
    {
        timescale = 1.0f;
        Time.timeScale = timescale;
        
        // Initialize checkpoint file path
        checkpointFilePath = Application.persistentDataPath + "/monster_agent_checkpoints.txt";
        Debug.Log("Checkpoint file path: " + checkpointFilePath);
        
        // Initialize TensorBoard logging
        statsRecorder = Academy.Instance.StatsRecorder;
    }

    void Update()
    {
        
    }
    
    private void SaveCheckpoint()
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string checkpointData = $"{timestamp}, Step: {stepCount}, Cumulative Reward: {GetCumulativeReward():F3}\n";
            
            System.IO.File.AppendAllText(checkpointFilePath, checkpointData);
            Debug.Log($"Checkpoint saved - Step: {stepCount}, Reward: {GetCumulativeReward():F3}");
            
            if (statsRecorder != null)
            {
                statsRecorder.Add("Monster/Cumulative_Reward_Checkpoint", GetCumulativeReward());
                statsRecorder.Add("Monster/Steps_Per_Checkpoint", stepCount);
                statsRecorder.Add("Monster/Episode_Number", episodeCount);
                
                if (monsterBehaviour != null)
                {
                    statsRecorder.Add("Monster/Speed_Boost_Uses", monsterBehaviour.speedBoostUses);
                    statsRecorder.Add("Monster/Speed_Boost_Active", monsterBehaviour.useSpeedBoost ? 1f : 0f);
                    statsRecorder.Add("Monster/Speed_Boost_Remaining", 5 - monsterBehaviour.speedBoostUses);
                    statsRecorder.Add("Monster/Current_Position", monsterBehaviour.position);
                    statsRecorder.Add("Monster/Timer_Difference", monsterBehaviour.timerDifference);
                    
                    statsRecorder.Add("Monster/Cooldown_Timer", monsterBehaviour.cooldownTimer);
                    statsRecorder.Add("Monster/Can_Choose_To_Spawn", monsterBehaviour.canChooseToSpawn ? 1f : 0f);
                    statsRecorder.Add("Monster/Waiting_For_Player", monsterBehaviour.waitingForPlayer ? 1f : 0f);
                }
                
                // Player FOV hit counts
                if (playerFOVRaycast != null && playerFOVRaycast.monsterLocationHitCounts != null)
                {
                    for (int i = 0; i < playerFOVRaycast.monsterLocationHitCounts.Length; i++)
                    {
                        statsRecorder.Add($"Monster/FOV_Hits_Location_{i}", playerFOVRaycast.monsterLocationHitCounts[i]);
                    }
                }
                
                // Puzzle progress
                if (gameFlow != null)
                {
                    statsRecorder.Add("Game/Wire_Puzzle_Timer", gameFlow.wirePuzzleTimer);
                    statsRecorder.Add("Game/Simon_Puzzle_Timer", gameFlow.simonPuzzleTimer);
                    statsRecorder.Add("Game/Cut_Puzzle_Timer", gameFlow.cutPuzzleTimer);
                    statsRecorder.Add("Game/Final_Wire_Puzzle_Timer", gameFlow.finalWirePuzzleTimer);
                }
                
                if (logManager != null)
                {
                    statsRecorder.Add("Game/Snapped_Components_Count", logManager.snappedNames.Count);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save checkpoint: {e.Message}");
        }
    }
    
    public override void OnEpisodeBegin()
    {
        if (monsterBehaviour.isAgentControlled == true)
        {
            // Reset checkpoint data and increment episode counter
            stepCount = 0;
            episodeCount++;
            
            timescale = 1.0f;
            Time.timeScale = timescale;
            monsterBehaviour.timer = 0f;
            monsterBehaviour.cooldownTimer = 0f; // Reset new cooldown system
            monsterBehaviour.canChooseToSpawn = false; // Reset spawn choice
            // Keep randomTimer for legacy compatibility
            monsterBehaviour.randomTimer = Random.Range(monsterBehaviour.timerLowerBound, monsterBehaviour.timerUpperBound);
            monsterBehaviour.waitingForPlayer = false;
            monsterBehaviour.speedBoostUses = 0;
            monsterBehaviour.useSpeedBoost = false;
            //Debug.Log("MonsterLocationHitCounts = " + playerFOVRaycast.monsterLocationHitCounts[0] + ", " + playerFOVRaycast.monsterLocationHitCounts[1] + ", " + playerFOVRaycast.monsterLocationHitCounts[2] + ", " + playerFOVRaycast.monsterLocationHitCounts[3]);
            playerFOVRaycast.monsterLocationHitCounts = new int[4] { 0, 0, 0, 0 };
            gameFlow.lightingLogic.ResetLight();
            monsterBehaviour.StartDelay = 8.0f;
            // Destroy green, red, and blue cells if they exist
            objectSpawner.DestroyAllInstances();

            //autoPlayer.OnEpisodeBegin();
            if (logManager != null)
            {
                logManager.ResetSnappedNames();
                logManager.ResetHolesAndSlots();
            }
            gameFlow.lightingLogic.StartGame();
            gameFlow.StartWirePuzzle();
            Debug.Log("Episode Begin");
            
            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string episodeData = $"{timestamp}, EPISODE BEGIN - Reset to Step 0, Episode: {episodeCount}\n";
                System.IO.File.AppendAllText(checkpointFilePath, episodeData);
                
                if (statsRecorder != null)
                {
                    statsRecorder.Add("Monster/Episode_Start", episodeCount);
                    statsRecorder.Add("Monster/Episode_Length_Previous", stepCount);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to log episode begin: {e.Message}");
            }
        }
        else
        {
            //Debug.LogError("MonsterBehaviour is not set to be Agent Controlled.");
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (!monsterBehaviour.isAgentControlled) return;
        sensor.AddObservation(playerFOVRaycast.monsterLocationHitCounts[0]);
        sensor.AddObservation(playerFOVRaycast.monsterLocationHitCounts[1]);
        sensor.AddObservation(playerFOVRaycast.monsterLocationHitCounts[2]);
        sensor.AddObservation(playerFOVRaycast.monsterLocationHitCounts[3]);


        GameObject[] monsterArr = monsterBehaviour.monsterArr;
        foreach (GameObject monster in monsterArr)
        {
            Vector3 pos = monster.transform.position;
            sensor.AddObservation(pos.x);
            sensor.AddObservation(pos.y);
            sensor.AddObservation(pos.z);
        }
        if (headCenter != null)
        {
            Vector3 camPos = headCenter.transform.position;
            sensor.AddObservation(camPos.x);
            sensor.AddObservation(camPos.y);
            sensor.AddObservation(camPos.z);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        if (gameFlow != null)
        {
            sensor.AddObservation(gameFlow.wirePuzzleTimer);
            sensor.AddObservation(gameFlow.simonPuzzleTimer);
            sensor.AddObservation(gameFlow.cutPuzzleTimer);
            sensor.AddObservation(gameFlow.peepersEventTimer);
            sensor.AddObservation(gameFlow.finalWirePuzzleTimer);
        }
        else
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }

        sensor.AddObservation(monsterBehaviour.timerDifference);
        sensor.AddObservation(monsterBehaviour.losingTimer);
        
        sensor.AddObservation(monsterBehaviour.cooldownTimer);
        sensor.AddObservation(monsterBehaviour.canChooseToSpawn);
        sensor.AddObservation(monsterBehaviour.cooldownTime);
        
        sensor.AddObservation(monsterBehaviour.randomTimer);

        sensor.AddObservation(monsterBehaviour.speedBoostUses);
        sensor.AddObservation(monsterBehaviour.useSpeedBoost);
        sensor.AddObservation(monsterBehaviour.position);
        sensor.AddObservation(monsterBehaviour.index);

        sensor.AddObservation(monsterBehaviour.waitingForPlayer);
        sensor.AddObservation(logManager.snappedNames.Count);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (!monsterBehaviour.isAgentControlled) return;
        
        // Branch 0: Location Choice (4 discrete actions: 0-3)
        int location = actionBuffers.DiscreteActions[0];
        int position = Mathf.Clamp(location, 0, 3); // Ensure valid range
        monsterBehaviour.position = position;

        // Branch 1: Spawn Decision (2 discrete actions: 0=don't spawn, 1=spawn now)
        int shouldSpawn = actionBuffers.DiscreteActions[1];
        
        // Branch 2: Speed Boost Usage (2 discrete actions: 0=don't use, 1=use)
        int speedBoost = actionBuffers.DiscreteActions[2];
        
        if (monsterBehaviour.canChooseToSpawn && !monsterBehaviour.waitingForPlayer && shouldSpawn == 1)
        {
            monsterBehaviour.AgentMonsterChoice();
            Debug.Log($"Agent chose to spawn at location {position}");
        }
        
        // Use the proper ActivateSpeedBoost method instead of directly setting the flag
        if (speedBoost == 1)
        {
            monsterBehaviour.ActivateSpeedBoost();
        }

        stepCount++;
        if (stepCount % checkpointInterval == 0)
        {
            SaveCheckpoint();
        }
    }

    public void HandleMonsterBeat(float reward)
    {
        AddReward(reward);
        Debug.Log("Monster Beat!");
        Debug.Log("Added " + reward + ". Cumulative Reward: " + GetCumulativeReward());
        
        if (statsRecorder != null)
        {
            statsRecorder.Add("Monster/Reward_MonsterBeat", reward);
            statsRecorder.Add("Monster/Cumulative_Reward_Event", GetCumulativeReward());
        }
    }
    
    public void HandleMonsterWin(float reward)
    {
        AddReward(reward);
        Debug.Log("Monster Win!");
        Debug.Log("Added " + reward + ". Cumulative Reward: " + GetCumulativeReward());

        if (statsRecorder != null)
        {
            statsRecorder.Add("Monster/Reward_MonsterWin", reward);
            statsRecorder.Add("Monster/Episode_Length_Final", stepCount);
            statsRecorder.Add("Monster/Cumulative_Reward_Final", GetCumulativeReward());
        }
        SaveCheckpoint();
        EndEpisode();
    }
    
    public void HandlePuzzleSolved(float reward)
    {
        AddReward(reward);
        Debug.Log("Puzzle Solved!");
        Debug.Log("Added " + reward + ". Cumulative Reward: " + GetCumulativeReward());

        if (statsRecorder != null)
        {
            statsRecorder.Add("Monster/Reward_PuzzleSolved", reward);
            statsRecorder.Add("Monster/Cumulative_Reward_Event", GetCumulativeReward());
        }
    }
    
    public void HandlePlayerWin(float reward)
    {
        AddReward(reward);
        Debug.Log("Player Win!");
        Debug.Log("Added " + reward + ". Cumulative Reward: " + GetCumulativeReward());
        
        if (statsRecorder != null)
        {
            statsRecorder.Add("Monster/Reward_PlayerWin", reward);
            statsRecorder.Add("Monster/Episode_Length_Final", stepCount);
            statsRecorder.Add("Monster/Cumulative_Reward_Final", GetCumulativeReward());
        }
        SaveCheckpoint();
        EndEpisode();
    }
    
}