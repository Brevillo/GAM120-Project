using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuSceneSelection : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI nameTextMesh, descriptionTextMesh;
    [SerializeField] private Button button;

    public void Setup(MainMenuSceneSelector.SceneInfo info) {

        nameTextMesh.text = info.name;
        descriptionTextMesh.text = info.description;
        button.onClick.AddListener(LoadScene);

        void LoadScene() => SceneManager.LoadScene(info.scene);
    }
}
