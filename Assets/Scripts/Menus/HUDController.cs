using System;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TMP_Text roundTimerText;
    [SerializeField] private TMP_Text globalTimerText;
    [SerializeField] private TMP_Text roundNumberText;
    
    [Header("Game Settings")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private Spawner spawner;

    private void Update()
    {
        // Actualizar el temporizador de la ronda
        float roundTime = gameTimer.currentTime;
        string roundTimeString = TimeSpan.FromSeconds(roundTime).ToString(@"mm\:ss");
        roundTimerText.text = "Siguiente Ronda: " + roundTimeString;
        
        // Actualizar el temporizador global
        float globalTime = gameTimer.globalTimer;
        string globalTimeString = TimeSpan.FromSeconds(globalTime).ToString(@"mm\:ss");
        globalTimerText.text = "Tiempo Global: " + globalTimeString;

        // Actualizar el número de ronda
        roundNumberText.text = "Ronda: " + spawner.currentRound;
    }
}