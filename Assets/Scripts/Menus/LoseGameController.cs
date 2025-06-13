using TMPro;
using UnityEngine;

public class LoseGameController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text bestRoundText;
    [SerializeField] private TMP_Text bestTimeText;
    
    [Header("References")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private Spawner spawner;
    [SerializeField] private PauseMenu pauseMenu;
    
    public void ShowPanel()
    {
        gameOverUI.SetActive(true);
        pauseMenu.PauseGame();
        SaveNewResult();
    }

    private void SaveNewResult()
    {
        // Obtener del PlayerPrefs el mejor tiempo y ronda
        int bestRound = PlayerPrefs.GetInt("BestRound", 0);
        string bestTime = PlayerPrefs.GetString("BestTime", "00:00");
        
        // Obtener el tiempo de la ronda actual
        float currentRoundTime = gameTimer.currentTime;
        string formattedRoundTime = FormatTime(currentRoundTime);
        
        // Obtener el número de ronda actual
        int currentRoundNumber = spawner.currentRound;

        // Actualizar los textos en la UI
        roundText.text = "Round: " + currentRoundNumber;
        timeText.text = "Time: " + formattedRoundTime;
        
        // Actualizar los mejores resultados
        bestRoundText.text = "Best Round: " + bestRound;
        bestTimeText.text = "Best Time: " + bestTime;
        
        // Guarda el nuevo mejor resultado si es necesario basado en la ronda
        if (currentRoundNumber > bestRound)
        {
            // Actualiza el mejor resultado guardando la ronda y el tiempo
            PlayerPrefs.SetInt("BestRound", currentRoundNumber);
            PlayerPrefs.SetString("BestTime", formattedRoundTime);
            // Guarda los cambios en PlayerPrefs
            PlayerPrefs.Save();
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        // Formatea el tiempo en minutos y segundos
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        // Devuelve el tiempo formateado como "mm:ss"
        return $"{minutes:D2}:{seconds:D2}";
    }
}