using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEditor.Compilation;

public static partial class LoadSceneMenuItemsGenerator {

    private const string
        MenuName = "Load Scene",
        GeneratedScriptName = "Generated Load Scene MenuItems";

    [MenuItem(MenuName + "/Generate MenuItems")]
    private static void GenerateMenuItems() {

        // get all scene assets and sort them based on folder

        Dictionary<string, List<SceneAsset>> sceneFolders = new();
        foreach (var (folder, asset) in AssetDatabase.FindAssets("t:scene", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => (folder: Path.GetDirectoryName(path), asset: AssetDatabase.LoadAssetAtPath<SceneAsset>(path))))

            if (sceneFolders.TryGetValue(folder, out var assets)) assets.Add(asset);
            else sceneFolders.Add(folder, new() { asset });

        // generate script

        string script = @" /* THIS CODE WAS AUTO GENERATED LOL */

using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class LoadSceneMenuItemsGenerator {

    private static void LoadScene(string name) {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(name);
    }

    private static class MenuItems {
";

        int priority = 0;

        foreach (var folder in sceneFolders.Values) {

            foreach (var scene in folder) {

                priority++;

                script += @$"
        [MenuItem(""{MenuName}/{scene.name}"", priority = {priority})]
        private static void LoadScene{priority}() => LoadScene(""{AssetDatabase.GetAssetPath(scene)}"");
";
            }

            priority += 10;
        }

        script += "    }\n}\n";

        // create script file

        string selfDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(
            AssetDatabase.FindAssets("t:script")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
            .First(script => script.GetClass() == typeof(LoadSceneMenuItemsGenerator)))),
               path = $"{selfDirectory}/{GeneratedScriptName}.cs";

        File.WriteAllText(path, script);
        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        CompilationPipeline.RequestScriptCompilation();
    }
}
