using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class BuildSettingsManager : ScriptableObject {

    [SerializeField] private SceneAsset startScene;
    [SerializeField] private DefaultAsset scenesFolder;

    #region Editor
    #if UNITY_EDITOR

    private void UpdateBuildSettings() {

        var paths = AssetDatabase.FindAssets("t:scene", new[] { AssetDatabase.GetAssetPath(scenesFolder) })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => new EditorBuildSettingsScene(path, true))
            .ToList();

        string startPath = AssetDatabase.GetAssetPath(startScene);

        paths.RemoveAll(scene => scene.path == startPath);
        paths.Insert(0, new EditorBuildSettingsScene(startPath, true));

        EditorBuildSettings.scenes = paths.ToArray();
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
