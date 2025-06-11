using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefab del enemigo")]
    public GameObject enemyPrefab;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;
    
    void Start()
    {
        // Spawnea un enemigo en cada punto de spawn
        foreach (Transform spawnPoint in spawnPoints)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Instantiate(enemyPrefab, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation); 
                       
        }
    }
    
}
