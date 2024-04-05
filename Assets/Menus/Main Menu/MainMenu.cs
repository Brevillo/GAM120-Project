using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Scene startScene;
    [SerializeField] private GameObject settingsContent;
    [SerializeField] private GameObject creditsContent;

    private enum State { None, Settings, Credits }
    private State state;

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(startScene);
    }

    private void ChangeState(State newState)
    {
        state = state != newState
            ? newState
            : State.None;

        settingsContent.SetActive(state == State.Settings);
        creditsContent.SetActive(state == State.Credits);
    }

    public void ToggleSettings()
    {
        ChangeState(State.Settings);
    }

    public void ToggleCredits()
    {
        ChangeState(State.Credits);
    }

    public void QuitGame()
    {
        Application.Quit();
        print("Quit Game");
    }
}
