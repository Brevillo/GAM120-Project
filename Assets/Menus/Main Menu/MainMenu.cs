using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Scene startScene;


    //start button
    //transition into test scene
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(startScene);
    }




    //exit button will close application

    public void EndGame()
    {
        Application.Quit();
        print("Quit Game");
    }







}
