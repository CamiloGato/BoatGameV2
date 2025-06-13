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
        float roundTime = gameTimer.currentTimeInRound;
        string roundTimeString = TimeSpan.FromSeconds(roundTime).ToString(@"mm\:ss");
        roundTimerText.text = "Next: " + roundTimeString;
        
        // Actualizar el temporizador global
        float globalTime = gameTimer.globalTimer;
        string globalTimeString = TimeSpan.FromSeconds(globalTime).ToString(@"mm\:ss");
        globalTimerText.text = "Time: " + globalTimeString;

        // Actualizar el número de ronda
        roundNumberText.text = "Round: " + spawner.currentRound;
    }
}