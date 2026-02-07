using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Oculus.Interaction.Body.Input;
public class PeepersBehaviour : MonoBehaviour
{
    public GameObject[] peepersArr;
    public float timer = 0f;
    public System.Collections.Generic.List<GameObject> activePeepers = new System.Collections.Generic.List<GameObject>();
    public bool waitingForPlayer = false;
    public float peepersWatchTime;
    public int peepersAmount;
    public int maxPeepersAmount = 7;
    public int maxActivePeepers = 2;
    public float lossTimer;
    public int position;
    public float timerDifference = 0f;
    public LightingLogic lightingLogic;
    public GameFlow gameFlow;
    public string deathSceneName;
    public bool LoadDeathScene;
    public Material seenMaterial;
    public Material defaultMaterial;
    public float peepersWaitTime;
    public bool runPeepers = false;
    public GameObject player;
    public Transform playerHead;
    private Vector3 target;
    
    public System.Collections.Generic.List<GameObject> recentlyUsedPeepers = new System.Collections.Generic.List<GameObject>();
    public int maxRecentlyUsed = 3;
    public float peeperCooldownTime = 5f;

    public AudioSource peeperSound;
    public AudioSource glitchSFX;

    void Start()
    {
        Debug.Log("PeepersBehaviour Start() called");
        //SpawnPeepers();
    }

    void Update()
    {
        CheckPeeperTimers();
        if (activePeepers.Count < maxActivePeepers && peepersAmount < maxPeepersAmount && runPeepers)
        {
            SpawnPeepers();
        }
        target = playerHead.position;
        for (int i = activePeepers.Count - 1; i >= 0; i--)
        {
            if (activePeepers[i] != null)
            {
                activePeepers[i].GetComponent<PeepersCollisionHandler>().LookAtPlayer(target);
            }
        }
        
    }
    
    void CheckPeeperTimers()
    {
        for (int i = activePeepers.Count - 1; i >= 0; i--)
        {
            if (activePeepers[i] != null)
            {
                PeepersCollisionHandler handler = activePeepers[i].GetComponent<PeepersCollisionHandler>();
                if (handler != null && handler.GetTimer() >= lossTimer)
                {
                    //Debug.Log("Peeper at index " + i + " exceeded loss timer!");
                    //PeeperWin();
                    //ResetPeepers();
                    if (LoadDeathScene)
                    {
                        Debug.Log("Player lost game because they failed to deter the peepers");
                        DeathIntegerAccess.deathInt = 3;
                        JumpScare js = FindFirstObjectByType<JumpScare>();
                        js.activate();   
                    }
                    return;
                }
            }
        }
    }
    
    void SpawnPeepers()
    {
        runPeepers = true;
        while (activePeepers.Count < maxActivePeepers && peepersAmount < maxPeepersAmount)
        {
            ActivateRandomPeeper();
        }
    }

    public void ResetPeepers()
    {
        Debug.Log("Resetting Peepers...");
        foreach (GameObject peeper in activePeepers)
        {
            if (peeper != null)
            {
                Destroy(peeper.GetComponent<PeepersCollisionHandler>());
                peeper.SetActive(false);
            }
        }
        runPeepers = true;
        activePeepers.Clear();
        peepersAmount = 0;
        waitingForPlayer = true;
    }
    public void DebugButtonPress()
    {
        Debug.Log("Debug Button Pressed - Activating Random Peeper");
        ActivateRandomPeeper();
        runPeepers = true;
    }
    public void ActivateRandomPeeper()
    {
        GameObject selectedPeeper = null;
        int attempts = 0;
        int maxAttempts = peepersArr.Length * 2;

        bool found = false;
        while (!found && attempts < maxAttempts)
        {
            int index = Random.Range(0, peepersArr.Length);
            selectedPeeper = peepersArr[index];
            attempts++;
            
            found = !activePeepers.Contains(selectedPeeper) && !recentlyUsedPeepers.Contains(selectedPeeper);
        }
        
        if (!found)
        {
            attempts = 0;
            while (!found && attempts < maxAttempts)
            {
                int index = Random.Range(0, peepersArr.Length);
                selectedPeeper = peepersArr[index];
                attempts++;
                found = !activePeepers.Contains(selectedPeeper);
            }
        }
        
        if (selectedPeeper == null)
        {
            Debug.LogError("Selected peeper is null!");
            return;
        }

        if (activePeepers.Contains(selectedPeeper))
        {
            Debug.LogWarning("All peepers are already active or no available peepers found!");
            return;
        }

        Debug.Log("Peeper Enabled: " + selectedPeeper.name);

        selectedPeeper.SetActive(true);

        PeepersCollisionHandler handler = selectedPeeper.GetComponent<PeepersCollisionHandler>();
        if (handler == null)
        {
            handler = selectedPeeper.AddComponent<PeepersCollisionHandler>();
        }

        activePeepers.Add(selectedPeeper);
        handler.Init(this);
        waitingForPlayer = true;

        Debug.Log("Active peeper added: " + selectedPeeper.name + ". Total active: " + activePeepers.Count);
    }

    public void PeeperCompleted(GameObject completedPeeper)
    {
        if (completedPeeper != null && activePeepers.Contains(completedPeeper))
        {
            Debug.Log("Peeper Disabled: " + completedPeeper.name);
            if (peeperSound != null)
            {
                peeperSound.Play();
            }
            Destroy(completedPeeper.GetComponent<PeepersCollisionHandler>());
            completedPeeper.SetActive(false);
            activePeepers.Remove(completedPeeper);
            
            AddToRecentlyUsed(completedPeeper);
        }

        peepersAmount++;

        Debug.Log("Peepers completed: " + peepersAmount + "/" + maxPeepersAmount + ". Active peepers: " + activePeepers.Count);

        if (peepersAmount >= maxPeepersAmount)
        {
            waitingForPlayer = false;
            runPeepers = false;

            if (activePeepers.Count == 0)
            {
                Debug.Log("Max Peeper Amount Reached! You win! YIPEEEEEEE");
                gameFlow.PeeperEventSolved();
            }
            
        }
        else
        {
            StartCoroutine(WaitAndCheckSpawn());
        }
    }
    public void PeeperWin()
    {
        Debug.Log("Peeper wins! Game over! You suck! Big Time!");
        waitingForPlayer = false;
        if (LoadDeathScene)
        {
            StartCoroutine(DeathSceneLoad());
        }
        runPeepers = false;
    }
    
    private IEnumerator DeathSceneLoad()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(deathSceneName);
    }
    
    private IEnumerator WaitAndCheckSpawn()
    {
        yield return new WaitForSeconds(peepersWaitTime);
    }
    
    private void AddToRecentlyUsed(GameObject peeper)
    {
        if (recentlyUsedPeepers.Contains(peeper))
        {
            recentlyUsedPeepers.Remove(peeper);
        }
        
        recentlyUsedPeepers.Insert(0, peeper);
        
        while (recentlyUsedPeepers.Count > maxRecentlyUsed)
        {
            recentlyUsedPeepers.RemoveAt(recentlyUsedPeepers.Count - 1);
        }
        
        StartCoroutine(RemoveFromRecentlyUsedAfterDelay(peeper));
    }
    
    private IEnumerator RemoveFromRecentlyUsedAfterDelay(GameObject peeper)
    {
        yield return new WaitForSeconds(peeperCooldownTime);
        
        if (recentlyUsedPeepers.Contains(peeper))
        {
            recentlyUsedPeepers.Remove(peeper);
            Debug.Log("Peeper " + peeper.name + " removed from recently used list - can spawn again");
        }
    }
}