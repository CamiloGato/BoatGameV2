using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject botonPausa;
    [SerializeField] private GameObject menuPausa;
    private bool juegoPausado = false;
    public AudioSource MusicPlayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Logica para pausar el juego
    public void PauseGame()
    {
        if (juegoPausado) return;
        
        juegoPausado = true;
        MusicPlayer.Pause();
        Time.timeScale = 0f;
        
        botonPausa.SetActive(false);
    }
    
    // Logica para mostrar el menu de pausa
    public void Pause()
    {
        PauseGame();
        menuPausa.SetActive(true);
    }

    // Logica para reanudar el juego
    public void Resume()
    {
        juegoPausado = false;
        MusicPlayer.UnPause();
        Time.timeScale = 1f;
        // Volver a mostrar el boton de pausa y ocultar el menu de pausa
        botonPausa.SetActive(true);
        menuPausa.SetActive(false);
    }

    // Logica para reiniciar el juego
    public void Restart()
    {
        Time.timeScale = 1f;
        // Reiniciar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Logica para cerrar el juego
    public void CloseGame()
    {
        Application.Quit();
    }
}
