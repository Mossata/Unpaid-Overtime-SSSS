using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LightingLogic : MonoBehaviour
{
    // Ceiling lights
    public Light southLight;
    public Light northLight;

    // Puzzle lights
    public Light wireLight;
    public Light simonLight;
    public GameObject simonStartButton;
    public GameObject[] WireStartButtons;
    public GameObject[] CircuitLights;

    // Desk lights
    public Light ambientDeskLight;
    public Light monitorLight;

    // Entity evil lights
    public Light evilDeskLight;
    public Light evilVentLight;
    public Light evilOverThereLight;
    public Light evilHallLight;
    public Light evilPeeperLight;
    public Light evilFinalPuzzleLight;

    // Puzzle Materials and Objects
    public Material redButtonOn;
    public Material redButtonOff;
    public Material blueButtonOn;
    public Material blueButtonOff;
    public Material circuitLightOn;
    public Material circuitLightOff;
    public GameObject monitor;
    public Material monitorScreenOn;
    public TextMeshProUGUI textElement;

    // Logic References
    public WirePuzzleLogic wirePuzzleLogic;
    public SimonLogic simonLogic;
    public PeepersBehaviour peepersBehaviour;
    public GameFlow gameFlow;
    public MonsterBehaviour monsterBehaviour;

    // Variables
    public bool inStartScene = false;

    // Anti Magic Numbers section cause i keep doi ng it

    // Delays and Durations
    private float flickerDelay = 0.2f;
    private float fadeDuration = 2.0f;
    private float ceilingLightFadeDuration = 1.0f;
    private float buttonTurnOnWait = 1.0f;
    private int flickerCount = 2;
    private float lightWaitTime = 3.0f;
    private float monsterLightDelay = 1.0f;
    private float noDelay = 0.0f;

    // Normal Light Target Intensities
    private float wireLightTargetIntensity = 0.15f;
    private float northSouthLightTargetIntensity = 0.075f;
    private float simonLightTargetIntensity = 0.06f;
    private float monitorLightTargetIntensity = 0.2f;
    private float ambientDeskLightTargetIntensity = 0.075f;
    private float offLightIntensity = 0.0f;
    private float FinalPuzzleAmbientIntensity = 0.5f;

    // Evil Light Target Intensities
    private float evilDeskLightTargetIntensity = 0.05f;
    private float evilOverThereLightTargetIntensity = 0.05f;
    private float evilHallLightTargetIntensity = 0.2f;
    private float evilVentLightTargetIntensity = 0.1f;
    private float evilPeeperLightTargetIntensity = 0.15f;

    // Puzzle Audio

    public AudioSource powerOutageSFX;
    public AudioSource wirePuzzleButtonsOn;
    public AudioSource wirePuzzleLightOn;
    public AudioSource monitorOnSFX;
    public AudioSource fnafLightSFX;
    public AudioSource peepersWinSFX;

    private void Start()
    {
        if (inStartScene)
        {
            StartScene();
        }
    }

    public void StartScene()
    {
        StartCoroutine(StartSceneSequence());
    }

    private IEnumerator StartSceneSequence()
    {
        yield return StartCoroutine(FadeLightIntensity(ambientDeskLight, offLightIntensity, ambientDeskLightTargetIntensity, fadeDuration));
        while (inStartScene)
        {
            yield return StartCoroutine(WaitFor(4.0f));
            yield return StartCoroutine(FlickerLight(ambientDeskLight, lightWaitTime, flickerCount, flickerDelay, ambientDeskLightTargetIntensity));
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        yield return StartCoroutine(MonitorOn());
        yield return StartCoroutine(FadeLightIntensity(monitorLight, offLightIntensity, monitorLightTargetIntensity, fadeDuration));

        textElement.enabled = true;
        textElement.text = "0/3 cells inserted...";

        yield return StartCoroutine(FlickerLight(ambientDeskLight, lightWaitTime, flickerCount, flickerDelay, ambientDeskLightTargetIntensity));

        yield return StartCoroutine(FadeLightIntensity(northLight, offLightIntensity, northSouthLightTargetIntensity, ceilingLightFadeDuration));
        yield return StartCoroutine(FadeLightIntensity(southLight, offLightIntensity, northSouthLightTargetIntensity, ceilingLightFadeDuration));

        yield return StartCoroutine(FadeLightIntensity(wireLight, offLightIntensity, wireLightTargetIntensity, fadeDuration));

        yield return StartCoroutine(StartWirePuzzle());

        monsterBehaviour.DelayTimerOn = true;

    }
    private IEnumerator StartWirePuzzle()
    {
        bool first = true;
        foreach (var button in WireStartButtons)
        {
            if (first && wirePuzzleLogic != null)
            {
                wirePuzzleButtonsOn.Play();
                first = false;
            }
            yield return changeMaterial(button, redButtonOn, buttonTurnOnWait);
        }
        foreach (var light in CircuitLights)
        {
            if (wirePuzzleLightOn != null)
            {
                wirePuzzleLightOn.Play();
            }
            yield return changeMaterial(light, circuitLightOn, noDelay);
        }
        yield return StartCoroutine(WaitFor(2.0f));
        gameFlow.StartWirePuzzle();
    }
    public void EndWirePuzzle()
    {
        StartCoroutine(EndWireSequence());
    }
    public IEnumerator EndWire()
    {
        foreach (var button in WireStartButtons)
        {
            yield return changeMaterial(button, redButtonOff, noDelay);
        }
        foreach (var light in CircuitLights)
        {
            yield return changeMaterial(light, circuitLightOff, noDelay);
        }
        wireLight.intensity = offLightIntensity;
    }
    private IEnumerator EndWireSequence()
    {
        yield return StartCoroutine(EndWire());
        
    }
    public void StartSimonPuzzle()
    {
        StartCoroutine(StartSimonSequence());
    }

    private IEnumerator StartSimonSequence()
    {
        yield return StartCoroutine(StartSimon());
        yield return StartCoroutine(FadeLightIntensity(simonLight, offLightIntensity, simonLightTargetIntensity, fadeDuration));
    }
    public IEnumerator StartSimon()
    {
        yield return changeMaterial(simonStartButton, blueButtonOn, buttonTurnOnWait);
    }
    public void EndSimonPuzzle()
    {
        StartCoroutine(EndSimonSequence());
    }
    private IEnumerator EndSimonSequence()
    {
        yield return StartCoroutine(EndSimon());
    }
    public IEnumerator EndSimon()
    {
        yield return changeMaterial(simonStartButton, blueButtonOff, noDelay);
        simonLight.intensity = offLightIntensity;
    }
    public void StartPeeperEvent()
    {
        StartCoroutine(StartPeeperSequence());
    }
    private IEnumerator StartPeeperSequence()
    {
        yield return StartCoroutine(FlickerLight(ambientDeskLight, lightWaitTime, flickerCount, flickerDelay, ambientDeskLightTargetIntensity));
        southLight.intensity = offLightIntensity;
        northLight.intensity = offLightIntensity;
        ambientDeskLight.intensity = offLightIntensity;
        if (!monsterBehaviour.isAgentControlled)
        {
            powerOutageSFX.Play();
        }
        
        yield return StartCoroutine(FadeLightIntensity(evilPeeperLight, offLightIntensity, evilPeeperLightTargetIntensity, fadeDuration * 2));
        gameFlow.PeeperStart();
    }

    public void EndPeeperEvent()
    {
        StartCoroutine(EndPeeperSequence());
    }
    private IEnumerator EndPeeperSequence()
    {
        yield return StartCoroutine(FlickerLight(evilPeeperLight, offLightIntensity, flickerCount, flickerDelay, evilPeeperLightTargetIntensity));
        evilPeeperLight.intensity = offLightIntensity;
        if (!monsterBehaviour.isAgentControlled)
        {
            peepersWinSFX.Play();
        }
        yield return StartCoroutine(FadeLightIntensity(ambientDeskLight, offLightIntensity, ambientDeskLightTargetIntensity, fadeDuration));
        yield return StartCoroutine(FadeLightIntensity(northLight, offLightIntensity, northSouthLightTargetIntensity, ceilingLightFadeDuration));
        yield return StartCoroutine(FadeLightIntensity(southLight, offLightIntensity, northSouthLightTargetIntensity, ceilingLightFadeDuration));

    }
    
    public void StartFinalPuzzle()
    {
        StartCoroutine(StartFinalPuzzleSequence());
    }
    private IEnumerator StartFinalPuzzleSequence()
    {
        yield return StartCoroutine(FlickerLight(ambientDeskLight, lightWaitTime, flickerCount, flickerDelay, ambientDeskLightTargetIntensity));
        southLight.intensity = offLightIntensity;
        northLight.intensity = offLightIntensity;
        ambientDeskLight.intensity = offLightIntensity;
        yield return StartCoroutine(FadeLightIntensity(evilFinalPuzzleLight, offLightIntensity, 0.05f, fadeDuration));
        gameFlow.finalWirePuzzleLogic.StartFinalPuzzle();
        
        // Change the ambient light intensity
        //RenderSettings.ambientIntensity = newAmbientIntensity;
    }

    public void EndFinalPuzzle()
    {
        StartCoroutine(EndFinalPuzzleSequence());
    }
    private IEnumerator EndFinalPuzzleSequence()
    {
        yield return StartCoroutine(FlickerLight(evilFinalPuzzleLight, lightWaitTime, flickerCount, flickerDelay, evilPeeperLightTargetIntensity));
        evilFinalPuzzleLight.intensity = offLightIntensity;
        yield return StartCoroutine(FadeLightIntensity(ambientDeskLight, offLightIntensity, ambientDeskLightTargetIntensity, fadeDuration));
        yield return StartCoroutine(FadeLightIntensity(northLight, offLightIntensity, northSouthLightTargetIntensity, ceilingLightFadeDuration));
        yield return StartCoroutine(FadeLightIntensity(southLight, offLightIntensity, northSouthLightTargetIntensity, ceilingLightFadeDuration));

    }

    
    public void GameWin()
    {
        Debug.Log("Game Won!");
        southLight.intensity = 3.0f;
        northLight.intensity = 3.0f;
    }
    public void MonsterStart()
    {
        StartCoroutine(MonsterSequence());
    }
    private IEnumerator MonsterSequence()
    {
        yield return StartCoroutine(FadeLightIntensity(evilDeskLight, offLightIntensity, evilDeskLightTargetIntensity, monsterLightDelay));
        yield return StartCoroutine(FadeLightIntensity(evilOverThereLight, offLightIntensity, evilOverThereLightTargetIntensity, monsterLightDelay));
        yield return StartCoroutine(FadeLightIntensity(evilHallLight, offLightIntensity, evilHallLightTargetIntensity, monsterLightDelay));
        yield return StartCoroutine(FadeLightIntensity(evilVentLight, offLightIntensity, evilVentLightTargetIntensity, monsterLightDelay));
    }

    private IEnumerator WaitFor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private IEnumerator FadeLightIntensity(Light targetLight, float startIntensity, float endIntensity, float duration)
    {
        if (targetLight == null)
        {
            Debug.LogError("Target light is null in FadeLightIntensity");
            yield break;
        }

        targetLight.intensity = startIntensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            targetLight.intensity = Mathf.Lerp(startIntensity, endIntensity, progress);
            yield return null;
        }

        targetLight.intensity = endIntensity;
    }

    private IEnumerator MonitorOn()
    {
        yield return new WaitForSeconds(2f);
        var renderer = monitor.GetComponent<Renderer>();
        Material[] materials = renderer.materials;
        
        if (materials.Length > 1)
        {
            if (monitorOnSFX != null)
            {
                monitorOnSFX.Play();
            }
            materials[1] = monitorScreenOn;
            renderer.materials = materials;
        }
        else
        {
            Debug.LogWarning("Monitor doesn't have a second material to replace");
        }
        
        yield return null;
    }

    private IEnumerator changeMaterial(GameObject thing, Material newMaterial, float delay)
    {
        
        if (thing == null)
        {
            yield break;
        }
        
        if (newMaterial == null)
        {
            yield break;
        }
        
        var renderer = thing.GetComponent<Renderer>();
        if (renderer == null)
        {
            yield break;
        }
        renderer.material = newMaterial;
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator FlickerLight(Light targetLight, float initialWait, int flickerCount, float lightWaitTime, float onIntensity)
    {
        if (targetLight == null)
        {
            Debug.LogError("Target light is null in FlickerLight");
            yield break;
        }

        yield return new WaitForSeconds(initialWait);
    
        for (int i = 0; i < flickerCount; i++)
        {
            Debug.LogError("Flicer Count: " + i);
            targetLight.intensity = 0f;
            yield return new WaitForSeconds(lightWaitTime);
            if (!monsterBehaviour.isAgentControlled)
            {
                if (fnafLightSFX != null)
                {
                    fnafLightSFX.Play();
                }
            }
   
            targetLight.intensity = onIntensity;
            yield return new WaitForSeconds(lightWaitTime);
        }
    }
    
    public void ResetLight()
    {
        StopAllCoroutines();
        ambientDeskLight.intensity = offLightIntensity;
        northLight.intensity = offLightIntensity;
        southLight.intensity = offLightIntensity;
        monitorLight.intensity = offLightIntensity;
        wireLight.intensity = offLightIntensity;
        simonLight.intensity = offLightIntensity;
        evilDeskLight.intensity = offLightIntensity;
        evilVentLight.intensity = offLightIntensity;
        evilOverThereLight.intensity = offLightIntensity;
        evilHallLight.intensity = offLightIntensity;
        evilPeeperLight.intensity = offLightIntensity;
        // textElement.enabled = false;
        foreach (var button in WireStartButtons)
        {
            var renderer = button.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = redButtonOff;
            }
        }
        foreach (var light in CircuitLights)
        {
            var renderer = light.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = circuitLightOff;
            }
        }
        //StartCoroutine(simonLogic.ShowRejectionDotLights());
    }
}