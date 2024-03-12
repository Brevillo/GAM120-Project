using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class BuildSettingsManager : ScriptableObject {

    [SerializeField] private List<Scene> scenes;

    #region Editor
    #if UNITY_EDITOR

    private void UpdateBuildSettings() {

        // get scene assets
        var allPaths = AssetDatabase.FindAssets("t:scene", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => (path, name: Path.GetFileNameWithoutExtension(path)))
            .ToList();

        var scenePaths = new List<string>();
        foreach (var scene in scenes) {
            int index = allPaths.FindIndex(info => info.name == scene);
            if (index != -1) scenePaths.Add(allPaths[index].path);
        }

        EditorBuildSettings.scenes = scenePaths.Select(scene => new EditorBuildSettingsScene(scene, true)).ToArray();
        
    }

    [CustomEditor(typeof(BuildSettingsManager))]
    private class BuildSettingsManagerEditor : Editor {

        private BuildSettingsManager manager => target as BuildSettingsManager;

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            if (GUILayout.Button("Update Build Settings"))
                manager.UpdateBuildSettings();
        }
    }

    #endif
    #endregion
}
