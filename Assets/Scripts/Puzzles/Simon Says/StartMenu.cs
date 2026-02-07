using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform spawnLocation;
    public string startSceneName;
    
    public void StartGame()
    {
        Debug.Log("Start Game button pressed.");
        StartCoroutine(StartSceneLoad());
    }

    private IEnumerator StartSceneLoad()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(startSceneName);
    }
    public void SpawnDonut()
    {
        Debug.Log("Spawn Donut button pressed.");
        Instantiate(prefabToSpawn, spawnLocation.position, Quaternion.identity);
    }
}