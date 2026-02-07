using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MonsterCollisionHandler : MonoBehaviour
{
    public MonsterBehaviour controller;
    private Coroutine collisionCoroutine;
    private Coroutine shakeCoroutine;
    public Material targetMaterial;
    public Texture2D textureA;
    public Texture2D textureB;
    private float timer = 0f;
    private Color previousCol = Color.yellow;
    
    private Vector3 originalPosition;
    private Vector3 originalTvFacePosition;
    private float minShakeIntensity = 0.01f; 
    private float maxShakeIntensity = 0.03f;
    private float currentShakeIntensity = 0.01f;
    private float shakeSpeed = 20f;
    private bool isBeingWatched = false;
    private int monsterIndex = -1;

    public void Init(MonsterBehaviour behaviour)
    {
        controller = behaviour;
        originalPosition = transform.position;
        for (int i = 0; i < controller.monsterArr.Length; i++)
        {
            if (controller.monsterArr[i] == gameObject)
            {
                monsterIndex = i;
                break;
            }
        }
        
        if (monsterIndex >= 0 && monsterIndex < controller.tvFaceArr.Length && controller.tvFaceArr[monsterIndex] != null)
        {
            originalTvFacePosition = controller.tvFaceArr[monsterIndex].transform.position;
        }
        
        if (monsterIndex == -1)
        {
            Debug.LogError($"Monster {gameObject.name} not found in monsterArr!");
        }
    }
    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= controller.losingTimer)
        {
            //GetComponent<Renderer>().material.color = Color.red;
            //previousCol = Color.red;
                //Normally swap to loss scene, for demo purposes, just change colour
            //SceneManager.LoadScene (sceneBuildIndex:2);
            //timer = 0f;
        }
        
        if (isBeingWatched)
        {
            ApplyShakeEffect();
        }
        else
        {
            transform.position = originalPosition;
            // Also reset tvFace position
            if (monsterIndex >= 0 && monsterIndex < controller.tvFaceArr.Length && controller.tvFaceArr[monsterIndex] != null)
            {
                controller.tvFaceArr[monsterIndex].transform.position = originalTvFacePosition;
            }
        }
    }

    public void OnSeenByPlayer()
    {
        if (collisionCoroutine == null)
        {
            controller.beingWatched = true;
            Debug.Log("Monster seen by player");
            controller.tvFaceRenderers[monsterIndex].GetComponent<Renderer>().material = controller.blueFace;
            // controller.tvFaceMaterial.TextureNumber = 1;
            //GetComponent<Renderer>().material.color = Color.blue;
            collisionCoroutine = StartCoroutine(WaitAndComplete());
        }
        
        if (!isBeingWatched)
        {
            isBeingWatched = true;
            if (shakeCoroutine == null)
            {
                shakeCoroutine = StartCoroutine(GraduallyIncreaseShake());
            }
        }
    }
    public void OnLostSightOfPlayer()
    {
        if (collisionCoroutine != null)
        {
            Debug.Log("Monster lost sight by player");
            
            controller.tvFaceRenderers[monsterIndex].GetComponent<Renderer>().material = controller.greenFace;
            
            // controller.tvFaceMaterial.SetFloat("_TextureNumber", 1);
            //GetComponent<Renderer>().material.color = previousCol;
            StopCoroutine(collisionCoroutine);
            collisionCoroutine = null;
        }
        
        controller.beingWatched = false;
        currentShakeIntensity = minShakeIntensity;
        transform.position = originalPosition;
        
        if (monsterIndex >= 0 && monsterIndex < controller.tvFaceArr.Length && controller.tvFaceArr[monsterIndex] != null)
        {
            controller.tvFaceArr[monsterIndex].transform.position = originalTvFacePosition;
        }
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    IEnumerator WaitAndComplete()
    {
        yield return new WaitForSeconds(controller.monsterWatchTime);
        controller.MonsterCompleted();
    }
    
    public void LookAtPlayer(Vector3 targetPosition)
    {
        if (targetPosition != Vector3.zero)
        {
            if (monsterIndex >= 0 && monsterIndex < controller.tvFaceArr.Length && controller.tvFaceArr[monsterIndex] != null)
            {
                Vector3 directionToPlayer = (targetPosition - controller.tvFaceArr[monsterIndex].transform.position).normalized;
                
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    targetRotation *= Quaternion.Euler(5f, 0f, 0f);
                    controller.tvFaceArr[monsterIndex].transform.rotation = Quaternion.Slerp(controller.tvFaceArr[monsterIndex].transform.rotation, targetRotation, Time.deltaTime * 2f);
                }
            }
        }
        else
        {
            Debug.LogError("Target (playerHead) is not assigned.");
        }
    }
    
    IEnumerator GraduallyIncreaseShake()
    {
        float shakeDuration = controller.monsterWatchTime;
        float shakeProgress = 0f;
        
        while (shakeProgress < shakeDuration && isBeingWatched)
        {
            shakeProgress += Time.deltaTime;
            
            float progressRatio = shakeProgress / shakeDuration;
            currentShakeIntensity = Mathf.Lerp(minShakeIntensity, maxShakeIntensity, progressRatio);
            
            yield return null;
        }
        
        shakeCoroutine = null;
    }
    
    
    private void ApplyShakeEffect()
    {
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) - 0.5f) * 2f * currentShakeIntensity;
        float shakeY = (Mathf.PerlinNoise(0, Time.time * shakeSpeed) - 0.5f) * 2f * currentShakeIntensity;
        float shakeZ = (Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) - 0.5f) * 2f * currentShakeIntensity;
        
        Vector3 shakeOffset = new Vector3(shakeX, shakeY, shakeZ);
        transform.position = originalPosition + shakeOffset;
        
        if (monsterIndex >= 0 && monsterIndex < controller.tvFaceArr.Length && controller.tvFaceArr[monsterIndex] != null)
        {
            controller.tvFaceArr[monsterIndex].transform.position = originalTvFacePosition + shakeOffset;
        }
    }
}