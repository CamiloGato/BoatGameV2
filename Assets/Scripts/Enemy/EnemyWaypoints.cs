using UnityEngine;

public class EnemyWaypoints : MonoBehaviour
{
    #region Singleton
    
    // Almacena una instancia única de EnemyWaypoints para que otros scripts puedan acceder a ella fácilmente
    public static EnemyWaypoints Instance;
    
    /// <summary>
    /// Puntos de referencia para los waypoints de los enemigos.
    /// </summary>
    [Header("Enemy Waypoints")]
    public Transform[] waypoints;
    
    private void Awake()
    {
        // Verifica si ya existe una instancia de EnemyWaypoints
        if (!Instance)
        {
            // Si no existe, asigna esta instancia como la única
            Instance = this;
        }
        else
        {
            // Si ya existe una instancia, destruye este objeto para evitar duplicados
            Destroy(gameObject);
        }
    }
    
    #endregion
}
