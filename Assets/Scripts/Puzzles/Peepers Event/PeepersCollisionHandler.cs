using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PeepersCollisionHandler : MonoBehaviour
{
    private PeepersBehaviour controller;
    private Coroutine fillCoroutine;

    private float timer = 0f;
    private Image scareImage;

    private float fillProgress = 0f;
    private bool isBeingWatched = false;
    private Transform peeperTransform;
    public Transform target;
    
    private Vector3 originalPosition;
    private float minShakeIntensity = 0.01f; 
    private float maxShakeIntensity = 0.03f;
    private float currentShakeIntensity = 0.01f;
    private float shakeSpeed = 20f;


    public void Init(PeepersBehaviour behaviour)
    {
        controller = behaviour;
        scareImage = GetComponentInChildren<Image>();
        timer = 0f;
        originalPosition = transform.position;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= controller.lossTimer)
        {
            //GetComponent<Renderer>().materials[0] = controller.seenMaterial;
        }
        
        if (isBeingWatched)
        {
            ApplyShakeEffect();
        }
        else
        {
            transform.position = originalPosition;
        }
    }
    
    public float GetTimer()
    {
        return timer;
    }
    public void LookAtPlayer(Vector3 targetPosition)
    {
        if (targetPosition != Vector3.zero)
        {
            Vector3 directionToPlayer = (targetPosition - transform.position).normalized;
            
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                targetRotation *= Quaternion.Euler(5f, 0f, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
        else
        {
            Debug.LogError("Target (playerHead) is not assigned.");
        }
    }
    public void OnSeenByPlayer()
    {
        Debug.Log("Peepers seen by player");
        if (controller.glitchSFX != null && !controller.glitchSFX.isPlaying)
        {
            controller.glitchSFX.Play();
        }
        if (!isBeingWatched)
        {
            isBeingWatched = true;
            //GetComponent<Renderer>().materials[0] = controller.seenMaterial;
            if (fillCoroutine == null)
            {
                Debug.Log("Starting GraduallyIncreaseShake coroutine");
                fillCoroutine = StartCoroutine(GraduallyIncreaseShake());
            }
        }
    }
    public void OnLostSightOfPlayer()
    {
        Debug.Log("Peepers lost sight by player");
        if (controller.glitchSFX != null && controller.glitchSFX.isPlaying)
        {
            controller.glitchSFX.Stop();
        }
        //GetComponent<Renderer>().materials[0] = controller.defaultMaterial;
        
        // Stop the fill coroutine if it's running
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
        
        isBeingWatched = false;
        fillProgress = 0f;
        currentShakeIntensity = minShakeIntensity;
        transform.position = originalPosition;
        
        if (scareImage != null)
        {
            scareImage.fillAmount = 0f;
        }
    }

    IEnumerator GraduallyIncreaseShake()
    {
        Debug.Log("GraduallyIncreaseShake coroutine started");
        if (scareImage != null)
        {
            scareImage.fillAmount = 0f;
        }
        
        float shakeDuration = 2f;
        fillProgress = 0f;
        
        while (fillProgress < shakeDuration && isBeingWatched)
        {
            fillProgress += Time.deltaTime;
            
            float progressRatio = fillProgress / shakeDuration;
            currentShakeIntensity = Mathf.Lerp(minShakeIntensity, maxShakeIntensity, progressRatio);
            
            // Update the UI fill amount if it exists
            if (scareImage != null)
            {
                scareImage.fillAmount = progressRatio;
            }
            
            yield return null;
        }
        
        if (isBeingWatched && fillProgress >= shakeDuration)
        {
            Debug.Log("Peeper shake completed - calling PeeperCompleted");
            controller.PeeperCompleted(gameObject);
        }
        else
        {
            Debug.Log("Peeper shake interrupted - player looked away");
        }
        
        fillCoroutine = null;
    }
    
    private void ApplyShakeEffect()
    {
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) - 0.5f) * 2f * currentShakeIntensity;
        float shakeY = (Mathf.PerlinNoise(0, Time.time * shakeSpeed) - 0.5f) * 2f * currentShakeIntensity;
        float shakeZ = (Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) - 0.5f) * 2f * currentShakeIntensity;
        
        Vector3 shakeOffset = new Vector3(shakeX, shakeY, shakeZ);
        transform.position = originalPosition + shakeOffset;
    }
}