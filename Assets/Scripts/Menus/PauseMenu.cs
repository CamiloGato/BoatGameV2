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
    
    public void Pause()
    {
        juegoPausado = true;
        MusicPlayer.Pause();
        Time.timeScale = 0f;
        botonPausa.SetActive(false);
        menuPausa.SetActive(true);     
       
    }

    public void Resume()
    {
        juegoPausado = false;
        MusicPlayer.UnPause();
        Time.timeScale = 1f;
        botonPausa.SetActive(true);
        menuPausa.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CloseGame()
    {
        Debug.Log("Cerrando juego");
        Application.Quit();
    }
}
