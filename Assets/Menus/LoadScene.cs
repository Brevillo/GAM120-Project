using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private Scene loadScene;

    public void DoLoadScene() => UnityEngine.SceneManagement.SceneManager.LoadScene(loadScene);
}
