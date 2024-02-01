using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Scene startScene;


    //start button
    //transition into test scene
    public void StartGame()
    {
        SceneManager.LoadScene(startScene);
    }




    //exit button will close application

    public void EndGame()
    {
        Application.Quit();
        print("Quit Game");
    }







}
