using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class MainMenuSceneSelector : MonoBehaviour {

    [System.Serializable]
    public class SceneInfo {
        public string name;
        public Scene scene;
        [TextArea(3, 99)] public string description;
    }

    [SerializeField] private List<SceneInfo> scenes;
    [SerializeField] private MainMenuSceneSelection selectionPrefab;
    [SerializeField] private RectTransform selectionContent;

    private void Start() {

        foreach (var scene in scenes)
            Instantiate(selectionPrefab, selectionContent).Setup(scene);
    }
}
