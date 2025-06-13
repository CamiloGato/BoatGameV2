using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefab del enemigo")]
    public GameObject enemyPrefab;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;
    
    [Header("Configuración de oleadas")]
    public int currentRound = 0;
    public int enemiesPerRound = 2;
    
    public void SpawnEnemies()
    {
        currentRound++; // Incrementa la ronda actual
        
        // Inicia la oleada de enemigos
        int totalEnemies = enemiesPerRound * currentRound;
        
        for (int i = 0; i < totalEnemies; i++)
        {
            // Selecciona un punto de spawn aleatorio
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Instantiate(enemyPrefab, spawnPoints[spawnIndex].position, Quaternion.identity);
        }
    }
}