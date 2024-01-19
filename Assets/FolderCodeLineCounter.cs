// Made by Oliver Beebe 2023
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

[CreateAssetMenu(fileName = "Folder Code Line Counter", menuName = "Folder Code Line Counter")]
public class FolderCodeLineCounter : ScriptableObject {

    [CustomEditor(typeof(FolderCodeLineCounter))]
    private class FolderCodeLineCounterEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var counter = target as FolderCodeLineCounter;
            EditorGUILayout.HelpBox(counter.info, MessageType.None, true);
        }
    }

    [SerializeField] private string folderPath;
    private string info;

    private void OnValidate() => CountLines();
    private void OnEnable() => CountLines();

    private void CountLines() {

        // gather all scripts
        List<MonoScript> scripts = new();
        List<string> subFolders = new() { $"Assets{(folderPath == "" ? "" : "/")}{folderPath}" };

        try {
            while (subFolders.Count > 0) {

                string path = subFolders[0];

                subFolders.Remove(path);
                subFolders.AddRange(AssetDatabase.GetSubFolders(path));

                new List<string>(Directory.GetFiles(path))
                    .ConvertAll(path => AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)))
                    .ForEach(asset => { if (asset != null && asset is MonoScript script) scripts.Add(script); });
            }
        } catch { }

        // sort
        var entries = scripts
            .ConvertAll(script => (name: script.name, lines: script.text.Split("\n").Length))
            .OrderBy(e => -e.lines);

        // count
        int total = entries.Sum(entry => entry.lines);

        // display
        info = $"Total: {total}\nScripts: {entries.ToArray().Length}\n{string.Join("\n", entries.Select((entry, i) => $"{i + 1}. {entry.lines} --- {entry.name}"))}";
    }
}

#endif
