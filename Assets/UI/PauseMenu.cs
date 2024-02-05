using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : Player.Component
{
    //stop time and open with escape button
    private bool paused;
    [SerializeField] private GameObject pauseMenuContent;
    [SerializeField] private Scene exitScene;

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        pauseMenuContent.SetActive(true);  
    }

    public void UnPause()
    {
        paused = false;
        Time.timeScale = 1;
        pauseMenuContent.SetActive(false);
    }

    public void Exit()
    {
        UnPause();
        UnityEngine.SceneManagement.SceneManager.LoadScene(exitScene);
    }

    private void Start()
    {
        pauseMenuContent.SetActive(false);
    }

    private void Update()
    {
        if (Input.Pause.Down)
        {
            if (paused)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
            
        }
    }

    //close with resume button or escape button




   //exit game or open main menu <- unpause before
   

}
