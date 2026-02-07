using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour
{
    public Light finalPuzzleLight;
    public Renderer finalPuzzleLightRenderer;
    public Material matON;
    public Material matOFF;
    
    public AudioSource flickerSF;
    public AudioSource switchSFX;
    
    public bool switchActive = true;
    
    private float flickerWaitTime = 0.15f;
    private float onIntensity = 0.3f;
    
    public void FinalPuzzleFlicker()
    {
        if (switchActive)
        {
            StartCoroutine(FinalPuzzleFlickerSequence());
        }
    }

    private void TurnBulbOn()
    {
        finalPuzzleLight.intensity = onIntensity;
        SetRendererMaterial(finalPuzzleLightRenderer, matON);
        if (flickerSF != null)
        {
            flickerSF.Play();
        }
    }
    
    private void TurnBulbOff()
    {
        finalPuzzleLight.intensity = 0f;
        SetRendererMaterial(finalPuzzleLightRenderer, matOFF);
    }
    
    private IEnumerator FinalPuzzleFlickerSequence()
    {
        switchActive = false;
        if (switchSFX != null)
        {
            switchSFX.Play();
        }
        
        if (finalPuzzleLight == null)
        {
            Debug.LogError("Final puzzle light is null in inspector");
            yield break;
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < 2; i++)
        {
            TurnBulbOn();
            yield return new WaitForSeconds(flickerWaitTime);

            TurnBulbOff();
            yield return new WaitForSeconds(flickerWaitTime);
        }
        
        TurnBulbOn();
        if (flickerSF != null)
        {
            flickerSF.loop = true;
            flickerSF.Play();
        }
        yield return new WaitForSeconds(4f);
        flickerSF.loop = false;
        
        for (int i = 0; i < 3; i++)
        {
            TurnBulbOff();
            yield return new WaitForSeconds(flickerWaitTime);
            
            TurnBulbOn();
            yield return new WaitForSeconds(flickerWaitTime);
        }
        if (switchSFX != null)
        {
            switchSFX.Play();
        }
        TurnBulbOff();
        switchActive = true;
        
    }
    
    private void SetRendererMaterial(Renderer rend, Material mat)
    {
        if (rend == null || mat == null) return;
        var maters = rend.materials;
        int idx = Mathf.Clamp(0, 0, maters.Length - 1);
        maters[idx] = mat;
        rend.materials = maters;
    }
}
