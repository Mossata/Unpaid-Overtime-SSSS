using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MonsterBehaviour : MonoBehaviour
{
    [Header("Monster Settings")]
    public bool useSpeedBoost = false;
    public int speedBoostUses = 0;
    private const int maxSpeedBoostUses = 5;
    public GameObject activeMonster = null;

    [Header("Renderers")]
    public GameObject[] monsterArr;
    public GameObject[] hitLocArr;
    public GameObject[] tvFaceArr;
    public GameObject[] tvFaceRenderers;
    public GameObject jumpscareFace;
    public MeshRenderer tvFaceRenderer;


    [Header("Materials")]
    public Material tvFaceMaterial;
    public Material redFace;
    public Material blueFace;
    public Material greenFace;

    [Header("Timer settings")]
    public float cooldownTime = 10f; // Minimum wait time before monster can choose to spawn
    public float cooldownTimer = 0f;
    public bool canChooseToSpawn = false;
    // Legacy fields kept for compatibility but will be phased out
    public float timerLowerBound; // Keep for non-agent mode
    public float timerUpperBound; // Keep for non-agent mode
    public float timer = 0f;
    public bool waitingForPlayer = false;
    public float monsterWatchTime;
    public float randomTimer = 0f; // Only used in non-agent mode now
    public float lossTime;
    public float losingTimer = 0f;
    public float StartDelay = 15.0f;
    public bool AllowSpawn = false;
    public float timerDifference = 0f;
    public bool DelayTimerOn = false;

    [Header("References")]

    public bool isAgentControlled;
    public int position;
    public MonsterAgent monsterAgent;
    public int index;
    public LightingLogic lightingLogic;
    public string deathSceneName;
    public bool LoadDeathScene;
    public GameObject lightingSystem;
    public float flickerTime = 0.2f;
    public Light computerLight;
    public GameObject player;
    public Transform playerHead;
    private Vector3 target;
    public bool beingWatched = false;
    public AudioSource monsterAudio;
    public AudioSource glitchSFX;
    public AudioSource heartbeatsSlow;
    public AudioSource heartbeatsNormal;
    public AudioSource heartbeatsFast;



    public void ActivateSpeedBoost()
    {
        if (speedBoostUses < maxSpeedBoostUses)
        {
            useSpeedBoost = true;
            //Debug.Log($"Speed boost activated! ({speedBoostUses + 1}/{maxSpeedBoostUses})");
        }
        else
        {
            //Debug.Log("No more speed boosts available.");
        }
    }



    void Update()
    {
        if (!AllowSpawn)
        {
            if (DelayTimerOn)
            {
                StartDelay -= Time.deltaTime;
            }
            if (StartDelay <= 0.0f)
            {
                AllowSpawn = true;
                lightingLogic.MonsterStart();
            }
            else
            {
                return;
            }
        }
        
        if (!waitingForPlayer)
        {
            if (isAgentControlled)
            {
                // Apply speed boost multiplier to cooldown timer if active
                float timeMultiplier = 1f;
                if (useSpeedBoost && speedBoostUses < maxSpeedBoostUses)
                {
                    timeMultiplier = 2f; // Double the cooldown reduction rate
                }
                
                cooldownTimer += Time.deltaTime * timeMultiplier;
                
                // After cooldown, monster can choose when to spawn
                if (cooldownTimer >= cooldownTime)
                {
                    canChooseToSpawn = true;
                    // Consume speed boost when cooldown is reached, not every frame
                    if (useSpeedBoost && speedBoostUses < maxSpeedBoostUses)
                    {
                        speedBoostUses++;
                        useSpeedBoost = false;
                        Debug.Log($"Speed boost consumed for faster cooldown! ({speedBoostUses}/{maxSpeedBoostUses})");
                    }
                    else if (useSpeedBoost && speedBoostUses >= maxSpeedBoostUses)
                    {
                        useSpeedBoost = false;
                        //Debug.Log("No more speed boosts available.");
                    }
                }
            }
            else
            {
                // Legacy random timer system for non-agent mode
                timer += Time.deltaTime;
                if (timer >= randomTimer)
                {
                    ActivateRandomMonster();
                    timer = 0f;
                }
            }
        }
        else
        {
            if (!beingWatched)
            {
                losingTimer += Time.deltaTime;
            }
            
            if (losingTimer >= lossTime)
            {
                MonsterWin();
                MonsterCompleted();
            }
        }

        if (activeMonster != null && playerHead != null)
        {
            target = playerHead.position;
            MonsterCollisionHandler handler = activeMonster.GetComponent<MonsterCollisionHandler>();
            if (handler != null)
            {
                handler.LookAtPlayer(target);
            }
        }

        if (activeMonster != null)
        {
            if (heartbeatsNormal != null && !heartbeatsNormal.isPlaying)
            {
                heartbeatsNormal.Play();
                heartbeatsSlow.Pause();
                heartbeatsFast.Pause();
            }
            if (beingWatched)
            {
                if (glitchSFX != null && !glitchSFX.isPlaying)
                {
                    glitchSFX.Play();
                }
            }
            else if (!beingWatched)
            {
                if (glitchSFX != null && glitchSFX.isPlaying)
                {
                    glitchSFX.Stop();
                }
            }
        }
        else
        {
            if (heartbeatsSlow != null && !heartbeatsSlow.isPlaying)
            {
                heartbeatsSlow.Play();
                heartbeatsFast.Pause();
                heartbeatsNormal.Pause();
            }
        }


    }
    void ActivateRandomMonster()
    {
        index = Random.Range(0, monsterArr.Length);
        activeMonster = monsterArr[index];

        activeMonster.SetActive(true);

        MonsterCollisionHandler handler = activeMonster.GetComponent<MonsterCollisionHandler>();
        if (handler == null)
        {
            handler = activeMonster.AddComponent<MonsterCollisionHandler>();
        }

        handler.Init(this);
        waitingForPlayer = true;
    }

    public void AgentMonsterChoice()
    {
        if (!canChooseToSpawn || waitingForPlayer) return;
        
        index = position;
        activeMonster = monsterArr[index];

        activeMonster.SetActive(true);

        MonsterCollisionHandler handler = activeMonster.GetComponent<MonsterCollisionHandler>();
        if (handler == null)
        {
            handler = activeMonster.AddComponent<MonsterCollisionHandler>();
        }
        hitLocArr[index].SetActive(false);

        handler.Init(this);
        waitingForPlayer = true;
        
        // Reset cooldown system
        canChooseToSpawn = false;
        cooldownTimer = 0f;
        
        Debug.Log($"Agent spawned monster at position {position}");
    }

    public void MonsterCompleted()
    {
        if (activeMonster != null)
        {
            StartCoroutine(FlickerHide());
        }

        timerDifference = lossTime - losingTimer;
        if (timerDifference > 0.0f)
        {
            Debug.Log("Scared away the monster just in time!");
        }
        if (isAgentControlled)
        {
            if (timerDifference >= lossTime / 2.0f)
            {
                monsterAgent.HandleMonsterBeat(0.02f);
            }
            else if (timerDifference >= lossTime / 3.0f)
            {
                monsterAgent.HandleMonsterBeat(0.04f);
            }
            else if (timerDifference >= lossTime / 4.0f)
            {
                monsterAgent.HandleMonsterBeat(0.06f);
            }
        }

        // Reset timers
        timer = 0f;
        losingTimer = 0f;
        waitingForPlayer = false;
        
        if (isAgentControlled)
        {
            cooldownTimer = 0f;
            canChooseToSpawn = false;
        }
        else
        {
            randomTimer = Random.Range(timerLowerBound, timerUpperBound);
            // Only consume speed boost if it was intentionally activated for timer reduction
            if (useSpeedBoost && speedBoostUses < maxSpeedBoostUses)
            {
                randomTimer *= 0.5f;
                speedBoostUses++;
                useSpeedBoost = false;
                Debug.Log($"Speed boost consumed for faster timer! ({speedBoostUses}/{maxSpeedBoostUses}) New timer: {randomTimer}");
            }
            else if (useSpeedBoost && speedBoostUses >= maxSpeedBoostUses)
            {
                useSpeedBoost = false;
                //Debug.Log("No more speed boosts available.");
            }
        }
    }

    public void MonsterWin()
    {
        monsterAgent.HandleMonsterWin(1.0f);
        if (LoadDeathScene)
        {
            tvFaceRenderers[index].GetComponent<Renderer>().material = redFace;
            Debug.Log("Player lost game because they failed to deter the monster");
            DeathIntegerAccess.deathInt = 1;
            JumpScare js = FindFirstObjectByType<JumpScare>();
            js.activate();
        }
    }

    private IEnumerator DeathSceneLoad()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(deathSceneName);
    }

    IEnumerator FlickerHide()
    {
        yield return new WaitForSeconds(flickerTime);
        if (!isAgentControlled)
        {
            monsterAudio.Play();
            glitchSFX.Stop();
        }

        if (activeMonster != null)
        {
            activeMonster.SetActive(false);
            hitLocArr[index].SetActive(true);
            activeMonster = null;
        }
    }
}
