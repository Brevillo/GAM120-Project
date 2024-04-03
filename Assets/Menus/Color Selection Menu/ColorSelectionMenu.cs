using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class ColorSelectionMenu : MonoBehaviour {

    [SerializeField] private ColorSelectionButton buttonPrefab;
    [SerializeField] private List<PlayerColorProfile> profiles;
    [SerializeField] private PlayerColorProfileReference playerColorProfileReference;
    [SerializeField] private Scene loadScene;
    [SerializeField] private RectTransform contentParent;

    private void Start() {
        foreach (var profile in profiles) {
            var button = Instantiate(buttonPrefab, contentParent).Initialize(profile);

            button.onClick.AddListener(() => {
                playerColorProfileReference.PlayerColorProfile = profile;
                UnityEngine.SceneManagement.SceneManager.LoadScene(loadScene);
            });
        }
    }
}
