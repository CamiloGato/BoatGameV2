using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    public float roundTimer = 60f; // Tiempo en segundos para cada ronda
    public float currentTimeInRound;
    public float globalTimer; // Temporizador global para todo el juego
    public UnityEvent onRoundEnd; // Evento que se dispara al finalizar una ronda
    
    private bool _isTimerActive = true; // Indica si el temporizador está activo
    
    private void Start()
    {
        currentTimeInRound = roundTimer; // Inicializa el temporizador con el tiempo de la ronda
        onRoundEnd.Invoke();
    }
    
    private void Update()
    {
        if (!_isTimerActive) return; // Si el temporizador no está activo, no hace nada
        
        // Actualiza el temporizador global
        globalTimer += Time.deltaTime;

        // Actualiza el temporizador de la ronda
        currentTimeInRound -= Time.deltaTime;

        // Comprueba si el temporizador de la ronda ha llegado a cero
        if (currentTimeInRound <= 0f)
        {
            // Dispara el evento de fin de ronda
            onRoundEnd.Invoke();
            currentTimeInRound = roundTimer; // Reinicia el temporizador de la ronda
        }
    }
    
    public void StopTimer()
    {
        _isTimerActive = false; // Detiene el temporizador
    }
}