using Unity.VisualScripting;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 offset = new Vector3(0.3f, 0, 0);

    private GameObject[] currentInstances;
    private int spawnIndex = 0;
    public AudioSource SpawnCellSF;

    public void RespawnObjects()
    {
        if (currentInstances == null)
        {
            currentInstances = new GameObject[prefabsToSpawn.Length];
        }

        if (spawnIndex < prefabsToSpawn.Length)
        {
            Vector3 spawnPos = spawnPoint.position;
            if (currentInstances[spawnIndex] != null)
            {
                Destroy(currentInstances[spawnIndex]);
            }

            StartCoroutine(PlaySoundAndSpawn(spawnPos));
        }
    }
    public void DestroyAllInstances()
    {
        if (currentInstances != null)
        {
            for (int i = 0; i < currentInstances.Length; i++)
            {
                if (currentInstances[i] != null)
                {
                    Destroy(currentInstances[i]);
                    currentInstances[i] = null;
                }
            }
        }
        spawnIndex = 0;
    }

    private System.Collections.IEnumerator PlaySoundAndSpawn(Vector3 pos)
    {
        if (SpawnCellSF != null)
        {
            SpawnCellSF.Play();
        }

        yield return new WaitForSeconds(7.5f);
        
        currentInstances[spawnIndex] = Instantiate(prefabsToSpawn[spawnIndex], pos, spawnPoint.rotation);
        spawnIndex++;
    }

}