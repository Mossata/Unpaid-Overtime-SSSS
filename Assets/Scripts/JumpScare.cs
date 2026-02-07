using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class JumpScare : MonoBehaviour
{
    public int lowerBound;
    public int upperBound;
    private bool activated = false;
    private float timer = 0f;
    private float randomTimer;
    public GameObject defaultCamera;
    public Camera jumpScareCamera;
    public AudioListener listener;
    public GameObject entity;


    public AudioSource jumpScareSFX;
    public GameObject lightingSystem;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        randomTimer = Random.Range(lowerBound, upperBound);
    }


    // Update is called once per frame
    void Update()
    {
        if (activated){
            lightingSystem.SetActive(false);
            timer += Time.deltaTime;
            if (timer >= randomTimer)
            {
                defaultCamera.SetActive(false);
                jumpScareCamera.enabled = true;
                entity.SetActive(true);
                listener.enabled = true;
                if (jumpScareCamera)
                {
                    jumpScareSFX.Play();
                }
                


                StartCoroutine(sceneLoadDelay());
            }
        }
    }


    public void activate()
    {
        activated = true;
    }
    private IEnumerator sceneLoadDelay()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Jumpscare complete");
        SceneManager.LoadScene(sceneBuildIndex:2);
    }
}
